using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repository
{
    public class ClubRepositoryAsync : GenericRepositoryAsync<Club>, IClubRepositoryAsync
    {
        private readonly DbSet<UserClub> _userClubs;
        private readonly DbSet<ClubJoinRequest> _joinRequests;
        private readonly DbSet<ApplicationUser> _users;
        private readonly ApplicationDbContext _dbContext;

        public ClubRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _userClubs = dbContext.Set<UserClub>();
            _joinRequests = dbContext.Set<ClubJoinRequest>();
            _users = dbContext.Set<ApplicationUser>();
        }

        public async Task<IReadOnlyList<ClubMemberDto>> GetClubMembersAsync(int clubId)
        {
            var query = from uc in _userClubs
                        join u in _users on uc.UserId equals u.Id
                        join r in _dbContext.ClubRoles on uc.ClubRoleId equals r.Id into roles
                        from r in roles.DefaultIfEmpty()
                        where uc.ClubId == clubId
                        select new ClubMemberDto
                        {
                            Id = u.Id,
                            Name = u.FullName,
                            Email = u.Email,
                            Role = r != null ? r.Name : "Member",
                            RoleColor = r != null ? (r.Name == "President" ? "#ef4444" : "#3b82f6") : "#94a3b8",
                            IsPresident = r != null && r.Name == "President",
                            Joined = uc.JoinDate
                        };

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<ClubJoinRequestDto>> GetClubJoinRequestsAsync(int clubId)
        {
            var query = from jr in _joinRequests
                        join u in _users on jr.UserId equals u.Id
                        where jr.ClubId == clubId && jr.Status == Core.Enums.ClubJoinStatus.Pending
                        select new ClubJoinRequestDto
                        {
                            Id = jr.Id,
                            UserId = u.Id,
                            Name = u.FullName,
                            Email = u.Email,
                            Message = "Join Request", // Maybe add message field to ClubJoinRequest entity later
                            Status = jr.Status.ToString()
                        };

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<Club>> GetManagedClubsAsync(string userId)
        {
            return await _userClubs
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.Club)
                .ToListAsync();
        }

        public async Task<ClubJoinRequest> GetJoinRequestByIdAsync(int requestId)
        {
            return await _joinRequests.FindAsync(requestId);
        }

        public async Task UpdateJoinRequestAsync(ClubJoinRequest joinRequest)
        {
            _dbContext.Entry(joinRequest).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}
