using System;
using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Chat
{
    public class ClubChannelDto
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public bool CanWrite { get; set; }
        public List<string> WriteRoleNames { get; set; } = new List<string>();
    }

    public class ChannelMessageDto
    {
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderRoleName { get; set; }
        public string SenderRoleColor { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class SendChannelMessageRequest
    {
        public string Content { get; set; }
    }

    public class CreateClubChannelRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> WriteRoleIds { get; set; } = new List<int>();
    }
}
