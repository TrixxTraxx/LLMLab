using Hangfire;
using LLMLab.Server.Data;
using LLMLab.Server.Service;
using LLMLab.Server.Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using LLMLab.Server.Mappers;

namespace LLMLab.Server.Jobs;

public class RunningJobInfo
{
    public Message Message { get; set; } = null!;
    public CancellationTokenSource CancellationTokenSource { get; set; } = null!;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

[AutomaticRetry(Attempts = 0)]
public class GenerateMessageJob(
    ApplicationDbContext dbContext,
    AiGenerationService aiGenerationService,
    ChatModelProvider chatModelProvider
)
{
    // Static dictionary to track currently running jobs by message ID
    private static readonly ConcurrentDictionary<int, RunningJobInfo> _runningJobs = new();

    // Public property to access running jobs from anywhere
    public static IReadOnlyDictionary<int, RunningJobInfo> RunningJobs => _runningJobs;
    
    private SemaphoreSlim _semaphore = new(1, 1);

    // Method to cancel a specific job by message ID
    public static bool CancelJob(int messageId)
    {
        if (_runningJobs.TryGetValue(messageId, out var jobInfo))
        {
            jobInfo.CancellationTokenSource.Cancel();
            return true;
        }
        return false;
    }

    public async Task GenerateMessageAsync(int messageId) 
    {
        Message? message = null;
        CancellationTokenSource cancellationTokenSource = new();
        
        try
        {
            message = await dbContext.Messages
                .Include(x => x.Thread)
                .ThenInclude(x => x.User)
                .Include(x => x.Attachments)
                .Include(x => x.Model)
                .FirstOrDefaultAsync(x => x.Id == messageId);
            
            if (message == null)
            {
                Console.WriteLine($"Message with ID {messageId} not found");
                return;
            }

            // Register this job in the running jobs dictionary
            var jobInfo = new RunningJobInfo
            {
                Message = message,
                CancellationTokenSource = cancellationTokenSource
            };
            _runningJobs.TryAdd(messageId, jobInfo);

            // Check for cancellation before proceeding
            cancellationTokenSource.Token.Register(() =>
            {
                message.Complete = true;
                SaveChangesLocked();
            });

            message.ModelResponse = string.Empty;
            await SaveChangesLockedAsync();
            
            var messageChain = GetMessageChain(message);
            
            var model = chatModelProvider.GetChatModel(message.Model);
            Console.WriteLine($"Using model: {message.Model.ModelId} for message {messageId} with provider {model.GetType().Name}");
            
            var result = await model.GenerateResponse(
                message,
                messageChain,
                message.Model,
                token =>
                {
                    SaveDebounced();
                    aiGenerationService.SendNewToken(messageId, token, false);
                },
                thinkingToken =>
                {
                    SaveDebounced();
                    aiGenerationService.SendNewToken(messageId, thinkingToken, true);
                },
                error =>
                {
                    message.Complete = true;
                    message.Error = true;
                    message.ErrorMessage = error;
                    SaveChangesLocked();
                    Task.Run(async () => {
                        try
                        {
                            await StopGeneration(message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send error notification for message {messageId}: {ex.Message}");
                        }
                    });
                },
                cancellationTokenSource.Token
            );
            
            // Mark message as complete
            message.Complete = true;
            await SaveChangesLockedAsync();
            
            await SaveResultAsync(message, result);
            
            // Only stop generation if there were no errors
            if (result == null || result.IsError)
            {
                // Handle error case
                Console.WriteLine($"Message generation failed for message {messageId}: {result?.ErrorMessage}");
            }

            await StopGeneration(message);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Generation job for message {messageId} was cancelled");
            
            // Mark message as cancelled
            if (message != null)
            {
                try
                {
                    message.Complete = true;
                    message.Error = true;
                    message.ErrorMessage = "Generation was cancelled";
                    await SaveChangesLockedAsync();
                    await StopGeneration(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save cancellation state for message {messageId}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in GenerateMessageAsync for message {messageId}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Mark message as complete even if there was an error to prevent infinite retries
            try
            {
                message!.Complete = true;
                message.ModelResponse = $"Error: {ex.Message}";
                await SaveChangesLockedAsync();
            }
            catch (Exception saveEx)
            {
                Console.WriteLine($"Failed to save error state for message {messageId}:");
                Console.WriteLine(saveEx);
            }
            
            try
            {
                await StopGeneration(message);
            }
            catch (Exception notifyEx)
            {
                Console.WriteLine($"Failed to send error notification for message {messageId}: {notifyEx.Message}");
            }
        }
        finally
        {
            // Always remove the job from the running jobs dictionary
            _runningJobs.TryRemove(messageId, out _);
            
            // Dispose the cancellation token source
            cancellationTokenSource?.Dispose();
        }
    }

    private async Task StopGeneration(Message message)
    {
        message.Complete = true;
        await SaveChangesLockedAsync();
        await aiGenerationService.SendStopMessage(message.Id, MessageMapper.Map(message));
    }

    private DateTime lastSaveTime = DateTime.MinValue;
    private void SaveDebounced()
    {
        var now = DateTime.UtcNow;
        if ((now - lastSaveTime).TotalSeconds < 1)
        {
            // If last save was less than 1 second ago, skip this save
            return;
        }
        
        lastSaveTime = now;
        SaveChangesLocked();
    }

    public void SaveChangesLocked()
    {
        _semaphore.Wait();
        try
        {
            dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving changes: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveChangesLockedAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving changes: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveResultAsync(Message message, ChatModelResponse? result)
    {
        try
        {
            // Save token usage and model information if needed
            var metadata = new MessageMetadata()
            {
                Message = message,
                ModelId = message.Model.ModelId,
                InputTokens = result?.InputTokens ?? 0,
                ThinkingTokens = result?.ThinkingTokens ?? 0,
                OutputTokens = result?.OutputTokens ?? 0,
                TotalInputTokenCost = message.Model.InputTokenCost * ((result?.InputTokens ?? 0d) / 1000000d),
                TotalThinkingTokenCost = message.Model.ThinkingCost * ((result?.ThinkingTokens ?? 0d) / 1000000d),
                TotalOutputTokenCost = message.Model.OutputTokenCost * ((result?.OutputTokens ?? 0d) / 1000000d),
                InputTokenCost = message.Model.InputTokenCost,
                ThinkingTokenCost = message.Model.ThinkingCost,
                OutputTokenCost = message.Model.OutputTokenCost,
                ModelName = message.Model.Name,
                ModelProvider = message.Model.Provider,
                UserId = message.Thread.UserId,
                CreatedAt = DateTime.UtcNow,
                UsedUserApiKey = false,
                ResponseTimeSeconds = (DateTime.UtcNow - message.CreatedAt).TotalSeconds,
                HasError = result?.IsError ?? false,
                ErrorMessage = result?.ErrorMessage ?? string.Empty
            };
            
            metadata.TotalCost = metadata.TotalInputTokenCost + metadata.TotalThinkingTokenCost + metadata.TotalOutputTokenCost;
            
            dbContext.MessageMetadata.Add(metadata);
            
            await SaveChangesLockedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving result for message {message.Id}");
            Console.WriteLine(ex);
        }
    }

    private List<Message> GetMessageChain(Message message)
    {
        try
        {
            var messages = dbContext.Messages
                .Include(x => x.Attachments)
                .Where(x => x.ThreadId == message.ThreadId && x.Id != message.Id)
                .OrderBy(x => x.CreatedAt)
                .ToList();
            
            var chain = new List<Message>();
            var currentMessage = message;
            while (currentMessage != null)
            {
                chain.Add(currentMessage);
                if (currentMessage.PreviousMessageId == null)
                {
                    break; // No previous message, end of chain
                }
                currentMessage = messages.FirstOrDefault(x => x.Id == currentMessage.PreviousMessageId);
            }
            
            return chain;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error building message chain for message {message.Id}: {ex.Message}");
            // Return at least the current message if chain building fails
            return new List<Message> { message };
        }
    }
}