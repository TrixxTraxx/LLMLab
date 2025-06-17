using System.Security.Claims;
using System.Text.Json;
using LLMLab.Dtos.Threads;
using LLMLab.Server.Data;
using LLMLab.Server.Mappers;
using Microsoft.EntityFrameworkCore;

namespace LLMLab.Server.Service;

public class ChatSharingService(
    ApplicationDbContext dbContext,
    IHttpContextAccessor httpContextAccessor
)
{
    public async Task<string> CreateSnapshot(int threadId)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var thread = await dbContext.MessageThreads.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == threadId && t.UserId == userId);
        if (thread == null)
            throw new BadHttpRequestException("Thread not found");

        var messages = await dbContext.Messages.Where(m => m.ThreadId == threadId).OrderBy(m => m.CreatedAt).ToListAsync();
        var messageDtos = messages.Select(MessageMapper.Map).ToList();

        var snapshot = new SharedChatSnapshotDto
        {
            Thread = new SharedThreadDto { Id = thread.Id, Title = thread.Title },
            Messages = messageDtos,
            Sharer = UserMapper.Map(thread.User)
        };
        var serialized = JsonSerializer.Serialize(snapshot);

        var shared = new SharedChat
        {
            Uuid = Guid.NewGuid(),
            UserId = userId,
            SerializedData = serialized
        };
        dbContext.SharedChats.Add(shared);
        await dbContext.SaveChangesAsync();
        var url = $"/share/{shared.Uuid}";
        return url;
    }

    public async Task<string> GetSharedChatData(Guid uuid)
    {
        var sharedChat = await dbContext.SharedChats.FirstOrDefaultAsync(s => s.Uuid == uuid);
        if (sharedChat == null)
            throw new BadHttpRequestException("Shared chat not found");

        return sharedChat.SerializedData;
    }
}