namespace CleanArchitecture.Core.Entities
{
    public class ClubChannelVisibilityRole
    {
        public int ChannelId { get; set; }
        public ClubChannel Channel { get; set; }

        public int ClubRoleId { get; set; }
        public ClubRole ClubRole { get; set; }
    }
}
