using System;
using System.Collections.Generic;
using LLMLab.Dtos.Messages;
using LLMLab.Dtos.User;

namespace LLMLab.Dtos.Threads
{
    public class SharedChatSnapshotDto
    {
        public SharedThreadDto Thread { get; set; }
        public List<MessageDto> Messages { get; set; }
        public UserDto Sharer { get; set; }
    }

    public class SharedThreadDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
} 