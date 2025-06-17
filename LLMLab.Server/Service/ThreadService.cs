using System.Security.Claims;
using LLMLab.Dtos.Threads;
using LLMLab.Server.Data;
using LLMLab.Server.Mappers;
using LLMLab.Server.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LLMLab.Server.Service;

public class ThreadService(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor,
    IHubContext<ThreadHub> hubContext
)
{
    public async Task<ThreadUpdateDto> GetThreads(int? clientVersion)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        var threads = context.MessageThreads
            .Where(x => x.UserId == userId && x.Version > clientVersion)
            .Select(x => new
            {
                //select the thread and its message Ids
                Thread = x,
                MessageIds = x.Messages.Select(m => m.Id).ToList()
            })
            .ToList();

        var threadDtos = threads.Select(thread =>
        {
            var dto = ThreadMapper.Map(thread.Thread);
            dto.MessageIds = thread.MessageIds;
            return dto;
        });
        
        return new ThreadUpdateDto()
        {
            UpdatedThreads = threadDtos.ToList(),
            UpdatedVersion = threads.Any() ? threads.Max(x => x.Thread.Version) : clientVersion ?? 0
        };
    }

    public async Task<ThreadDto?> UpdateThread(ThreadDto threadDto)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var thread = await context.MessageThreads
            .Include(x => x.User)
            .FirstOrDefaultAsync(t => t.Id == threadDto.Id && t.UserId == userId);
        
        if (thread == null)
        {
            return null; // Thread not found or does not belong to the user
        }

        thread.User.ThreadVersion += 1; // Increment user's thread version
        thread.Title = threadDto.Title;
        thread.UpdatedAt = DateTime.UtcNow;
        thread.Deleted = threadDto.Deleted;
        thread.Version = thread.User.ThreadVersion; // Update thread version
        
        await context.SaveChangesAsync();
        SendThreadUpdate(userId);
        
        return ThreadMapper.Map(thread);
    }

    public async Task<int> CreateChatBranch(int messageId)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var thread = await context.MessageThreads
            .Include(x => x.Messages)
            .Include(x => x.User)
            .FirstOrDefaultAsync(thread => thread.Messages.Any(m => m.Id == messageId) && thread.UserId == userId);
        
        if (thread == null)
        {
            throw new KeyNotFoundException($"Message with ID {messageId} not found.");
        }

        thread.User.ThreadVersion++; // Increment user's thread version
        // Create a new thread for the chat branch
        var newThread = new MessageThread
        {
            UserId = userId,
            Title = thread.Title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BranchFromThreadId = thread.Id, // Link to the original thread
            Version = thread.User.ThreadVersion // Initial version
        };
        
        var messages = new List<Message>();
        // get all the messages in the message chain
        var currentMessage = thread.Messages.
            First(m => m.Id == messageId);
        while (currentMessage != null)
        {
            // Create a copy of the message for the new thread by detaching it from the context
            context.Entry(currentMessage).State = EntityState.Detached;
            currentMessage.Id = 0; // Reset ID for the new thread
            messages.Add(currentMessage);
            if (currentMessage.PreviousMessageId == 0) break;
            currentMessage = thread.Messages
                .FirstOrDefault(m => m.Id == currentMessage.PreviousMessageId);
        }
        newThread.Messages = messages;
        
        context.MessageThreads.Add(newThread);
        
        // Add the message to the new thread
        await context.SaveChangesAsync();

        SendThreadUpdate(userId);
        
        return newThread.Id; // Return the ID of the newly created thread
    }
    
    public void SendThreadUpdate(string userId)
    {
        // Notify all clients about the updated thread
        hubContext.Clients.Group(userId).SendAsync("ThreadsUpdated");
    }
}