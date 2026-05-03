using System;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class ClubJoinRequestDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
    }
}
