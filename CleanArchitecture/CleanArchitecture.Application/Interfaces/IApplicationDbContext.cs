using CleanArchitecture.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<RefreshToken> RefreshTokens { get; set; }
        DbSet<Event> Events { get; set; }
        DbSet<EventClub> EventClubs { get; set; }
        DbSet<Club> Clubs { get; set; }
        DbSet<ClubRole> ClubRoles { get; set; }
        DbSet<ClubPrivilege> ClubPrivileges { get; set; }
        DbSet<ClubRolePrivilege> ClubRolePrivileges { get; set; }
        DbSet<UserClub> UserClubs { get; set; }
        DbSet<ClubJoinRequest> ClubJoinRequests { get; set; }
        DbSet<EventAttendee> EventAttendees { get; set; }
        DbSet<Asset> Assets { get; set; }
        DbSet<AssetLoan> AssetLoans { get; set; }
        DbSet<BudgetRequest> BudgetRequests { get; set; }
        DbSet<Announcement> Announcements { get; set; }
        DbSet<ClubCreationRequest> ClubCreationRequests { get; set; }
        DbSet<ClubCreationRequestSupporter> ClubCreationRequestSupporters { get; set; }
        DbSet<ChatMessage> ChatMessages { get; set; }
        DbSet<ClubChannel> ClubChannels { get; set; }
        DbSet<ClubChannelMessage> ClubChannelMessages { get; set; }
        DbSet<ClubChannelWriteRole> ClubChannelWriteRoles { get; set; }
        DbSet<ClubChannelVisibilityRole> ClubChannelVisibilityRoles { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}
