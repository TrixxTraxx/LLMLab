using System;
using System.Collections.Generic;

namespace LLMLab.Server.Data
{
    public class SharedChat
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; }
        public string SerializedData { get; set; } // JSON of SharedChatSnapshot
    }
} 