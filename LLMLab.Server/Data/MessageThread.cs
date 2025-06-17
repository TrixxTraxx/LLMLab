namespace LLMLab.Server.Data;

public class MessageThread
{
    public int Id { get; set; }
    public string Title { get; set; }
    
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    // The ID of the last message in the thread
    // we could make this a foreign key but its not strictly necessary since its a deep copy and only for display purposes
    public int? BranchFromThreadId { get; set; } = null;
    
    public int Version { get; set; } = 1;
    
    // The date and time when the thread was created
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // The date and time when the thread was last updated
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool Deleted { get; set; }
}