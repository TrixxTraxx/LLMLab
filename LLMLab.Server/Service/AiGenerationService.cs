using Hangfire;
using LLMLab.Dtos.Messages;
using LLMLab.Server.Data;
using LLMLab.Server.Jobs;
using LLMLab.Server.Mappers;
using LLMLab.Server.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LLMLab.Server.Service;

public class AiGenerationService(
    ApplicationDbContext dbContext,
    IHubContext<MessageHub> hubContext
)
{
    public string StartGeneration(int messageId)
    {
        return BackgroundJob.Enqueue<GenerateMessageJob>(x => x.GenerateMessageAsync(messageId));
    }

    public async Task GenerateThreadTitle(int threadId, string newMessageText)
    {
        BackgroundJob.Enqueue<GenerateThreadTitleJob>(x => x.GenerateThreadTitleAsync(threadId, newMessageText));
    }

    public async Task StopGeneration(int messageId)
    {
        var jobInfo = GenerateMessageJob.RunningJobs.GetValueOrDefault(messageId);

        if (jobInfo == null)
        {
            var message = await dbContext.Messages
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == messageId);
            if (message == null)
            {
                throw new ArgumentException("Message not found", nameof(messageId));
            }
            
            message.Complete = true;
            await dbContext.SaveChangesAsync();

            // Notify clients that the generation has stopped
            await SendStopMessage(messageId, MessageMapper.Map(message));
        }
        else
        {
            // cancel the job
            GenerateMessageJob.CancelJob(messageId);

            // Notify clients that the generation has stopped
            await SendStopMessage(messageId, MessageMapper.Map(jobInfo.Message));
        }
    }

    public async Task SendStopMessage(int messageId, MessageDto messageDto)
    {
        await hubContext.Clients.Group(messageId.ToString()).SendAsync("GenerationStopped", messageDto);
    }


    public async Task SendExistingMessage(ApplicationUser user, int messageId, HubCallerContext context)
    {
        try
        {
            var jobInfo = GenerateMessageJob.RunningJobs.GetValueOrDefault(messageId);

            if (jobInfo == null)
            {
                // If the message is complete, notify the client
                await StopGeneration(messageId);
                return;
            }
            else
            {
                // Send the existing message to the client
                await hubContext.Clients.Client(context.ConnectionId)
                    .SendAsync("NewMessage", MessageMapper.Map(jobInfo.Message));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    
    public void SendNewToken(int messageId, string token, bool isThinkingToken)
    {
        // get all Clients for the messageId
        var clients = hubContext.Clients.Group(messageId.ToString());
        if (clients == null)
        {
            throw new ArgumentException("No clients found for the given messageId", nameof(messageId));
        }
        
        // Send the new token to all clients in the group
        clients.SendAsync("ReceiveNewToken", token, isThinkingToken).GetAwaiter().GetResult();
    }
}