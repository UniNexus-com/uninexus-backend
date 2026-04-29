using System.Collections.Generic;

namespace CleanArchitecture.Core.Entities
{
    public class ClubChannel : AuditableBaseEntity
    {
        public int ClubId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }

        public Club Club { get; set; }
        public ICollection<ClubChannelMessage> Messages { get; set; } = new List<ClubChannelMessage>();
        public ICollection<ClubChannelWriteRole> WriteRoles { get; set; } = new List<ClubChannelWriteRole>();
    }
}
