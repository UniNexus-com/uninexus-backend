using System;
using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Event
{
    public class EventViewModel
    {
        public int Id { get; set; }
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
        public string CoverImageUrl { get; set; }

        /// <summary>Üniversite | Club | JointClub — host küme sayısına göre.</summary>
        public string OrganizerKind { get; set; }

        public List<EventHostBriefDto> HostClubs { get; set; } = new List<EventHostBriefDto>();

        /// <summary>Tek kulüpte birincil id; ortak düzende genelde ilk sıra kulüp null olabilir (HostClubs kullanın).</summary>
        public int? ClubId { get; set; }

        /// <summary>Tek kulüpte adı; ortak düzende güvenilir gösterim için HostClubs önerilir.</summary>
        public string ClubName { get; set; }

        /// <summary>Mevcut kullanıcı bu etkinliğe kayıt oldu mu? (Anonim/yetkisiz isteklerde false.)</summary>
        public bool IsRegistered { get; set; }
    }
}
