namespace CleanArchitecture.Core.Entities
{
    public class ClubRolePrivilege
    {
        public int ClubRoleId { get; set; }
        public ClubRole ClubRole { get; set; }

        public int PrivilegeId { get; set; }
        public ClubPrivilege Privilege { get; set; }
    }
}
