using CleanArchitecture.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Entities
{
    public class Announcement : AuditableBaseEntity
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public AnnouncementPriority Priority { get; set; }
        public int? ClubId { get; set; }
    }
}
