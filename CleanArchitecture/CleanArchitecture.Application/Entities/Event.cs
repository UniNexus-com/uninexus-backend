using System;
using System.Collections.Generic;

namespace CleanArchitecture.Core.Entities
{
    public class Event : AuditableBaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
        public string Visibility { get; set; }
        public int? Capacity { get; set; }
        public string Requirements { get; set; }
        public bool RequireApproval { get; set; }
        public string Tags { get; set; }
        public string CoverImageUrl { get; set; }

        public ICollection<EventClub> EventClubs { get; set; } = new List<EventClub>();
    }
}
