using System.Text.Json;
using LLMLab.Client.Caches;

namespace LLMLab.Client.Models;

public class MessageTreeDto
{
    public ThreadCache Thread { get; set; } = new ThreadCache();
    public Dictionary<int, MessageCache> Messages { get; set; } = new ();
    
    public int StartMessageId { get; set; } = 0;
    
    public Dictionary<int, MessageCache> PreviousMessages { get; set; } = new ();
    public Dictionary<int, List<MessageCache>> NextMessages { get; set; } = new ();

    public List<MessageCache> GetMessagesForBranch(int currentBranchPath)
    {
        var messages = new List<MessageCache>();
        if (!Messages.TryGetValue(currentBranchPath, out var message))
        {
            // If the current branch path is not found, return an empty list
            return messages;
        }
        
        while(message != null)
        {
            messages.Add(message);
            message = PreviousMessages.GetValueOrDefault(message.Message.Id);
        }
        
        return messages;
    }
    
    //get the pagination info for a specific message
    public (int? nextId, int? previosId, int count, int index) GetPaginationInfo(int messageId)
    {
        //Console.WriteLine("Calculating pagination info for messageId: " + messageId + " and Message Three: " + JsonSerializer.Serialize(this));
        var previousMessage = PreviousMessages.GetValueOrDefault(messageId);
        if (previousMessage == null)
        {
            Console.WriteLine("No previous message found, considering the root message.");
            var messageKeys = Messages
                .Where(x => x.Value.Message.PreviousMessageId == 0)
                .OrderBy(x => x.Value.Message.CreatedAt)
                .Select(x => x.Key)
                .ToList();
            return ConvertMessageKeysIntoPaginationInfo(messageKeys, messageId);
        }
        var nextMessages = NextMessages.GetValueOrDefault(previousMessage.Message.Id);
        
        Console.WriteLine("Found messages at same level: " + (string.Join(", ", NextMessages.Select(x => $"{x.Key}: {string.Join(", ", x.Value.Select(m => m.Message.Id))}"))));
        return ConvertMessageKeysIntoPaginationInfo(nextMessages?
                .OrderBy(x => x.Message.CreatedAt)
                .Select(x => x.Message.Id)
                .ToList(),
            messageId);
    }

    private (int? nextId, int? previosId, int count, int index) ConvertMessageKeysIntoPaginationInfo(List<int>? messageKeys,
        int messageId)
    {
        if (messageKeys == null || messageKeys.Count == 0)
        {
            Console.WriteLine("MessageKeys Empty or null for messageId: " + messageId);
            return (null, null, 0, 0);
        }
        
        var index = messageKeys.IndexOf(messageId);
        if (index < 0)
        {
            Console.WriteLine("found invalid Message Tree pagination info for messageId: " + messageId);
            return (null, null, 0, 0);
        }

        int? nextId = index < messageKeys.Count - 1 ? messageKeys[index + 1] : null;
        int? previousId = index > 0 ? messageKeys[index - 1] : null;
        
        Console.WriteLine("found Pagination Info: " + nextId + ", " + previousId + ", " + messageKeys.Count + ", " + index);
        return (nextId, previousId, messageKeys.Count, index);
    }

    public int GetBranchFromMessage(int id)
    {
        var message = Messages.Values.First(m => m.Message.Id == id);
        //find the newest message
        while (true)
        {
            var nextMessages = NextMessages.GetValueOrDefault(message!.Message.Id);
            if (nextMessages != null && nextMessages.Count > 0)
            {
                // If there are next messages, return the first one
                message = nextMessages.MaxBy(x => x.Message.CreatedAt);
            }
            else
            {
                return message.Message.Id;
            }
        }
    }
}