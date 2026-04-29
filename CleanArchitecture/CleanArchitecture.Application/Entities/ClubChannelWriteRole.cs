namespace CleanArchitecture.Core.Entities
{
    public class ClubChannelWriteRole
    {
        public int ChannelId { get; set; }
        public ClubChannel Channel { get; set; }

        public int ClubRoleId { get; set; }
        public ClubRole ClubRole { get; set; }
    }
}
