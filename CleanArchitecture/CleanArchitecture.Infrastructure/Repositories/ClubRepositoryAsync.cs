using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
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
        private readonly DbSet<ClubRole> _clubRoles;
        private readonly ApplicationDbContext _dbContext;

        public ClubRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _userClubs = dbContext.Set<UserClub>();
            _joinRequests = dbContext.Set<ClubJoinRequest>();
            _users = dbContext.Set<ApplicationUser>();
            _clubRoles = dbContext.Set<ClubRole>();
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
                            RoleId = uc.ClubRoleId,
                            RoleColor = r != null ? (r.Color ?? "#3b82f6") : "#94a3b8",
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
                .Include(uc => uc.Club)
                .Include(uc => uc.Role)
                .Where(uc => uc.UserId == userId
                    && uc.IsActive
                    && uc.Role != null
                    && uc.Role.Name != "Active Member")
                .Select(uc => uc.Club)
                .ToListAsync();
        }

        public async Task<bool> HasPendingJoinRequestAsync(int clubId, string userId)
            => await _joinRequests.AnyAsync(jr => jr.ClubId == clubId && jr.UserId == userId && jr.Status == Core.Enums.ClubJoinStatus.Pending);

        public async Task<bool> IsClubMemberAsync(int clubId, string userId)
            => await _userClubs.AnyAsync(uc => uc.ClubId == clubId && uc.UserId == userId);

        public async Task RemoveMemberAsync(int clubId, string userId)
        {
            var userClub = await _userClubs
                .FirstOrDefaultAsync(uc => uc.ClubId == clubId && uc.UserId == userId);
            if (userClub == null) throw new KeyNotFoundException("Member not found in this club.");
            _userClubs.Remove(userClub);
            await _dbContext.SaveChangesAsync();
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

        public async Task<MemberDetailsDto> GetClubMemberDetailsAsync(int clubId, string userId)
        {
            var query = from uc in _userClubs
                        join u in _users on uc.UserId equals u.Id
                        join r in _dbContext.ClubRoles on uc.ClubRoleId equals r.Id into roles
                        from r in roles.DefaultIfEmpty()
                        where uc.ClubId == clubId && uc.UserId == userId
                        select new MemberDetailsDto
                        {
                            Id = u.Id,
                            Name = u.FullName,
                            Email = u.Email,
                            StudentNumber = u.StudentNumber,
                            Role = r != null ? r.Name : "Member",
                            RoleColor = r != null ? (r.Name == "President" ? "#ef4444" : "#3b82f6") : "#94a3b8",
                            IsPresident = r != null && r.Name == "President",
                            Joined = uc.JoinDate,
                            Phone = u.PhoneNumber, // Default fields from IdentityUser
                            Major = "Computer Engineering", // Dummy for now as it's not in DB
                            Year = "3rd Year", // Dummy for now
                            Bio = "Bio placeholder" // Dummy for now
                        };

            return await query.FirstOrDefaultAsync();
        }
        public async Task<ClubStatsDto> GetClubStatsAsync(int clubId)
        {
            var now = DateTime.UtcNow;
            var oneYearAgo = now.AddYears(-1);

            var club = await _dbContext.Clubs.FindAsync(clubId);
            if (club == null) return null;

            var stats = new ClubStatsDto
            {
                TotalMembers = await _userClubs.CountAsync(uc => uc.ClubId == clubId),
                UpcomingEventsCount = await _dbContext.Events.CountAsync(e => e.ClubId == clubId && e.StartDate > now && e.IsActive),
                TotalBudget = club.TotalBudget ?? 0
            };

            // Calculate Growth Rate (Last 30 days)
            var thirtyDaysAgo = now.AddDays(-30);
            var newMembersCount = await _userClubs.CountAsync(uc => uc.ClubId == clubId && uc.JoinDate >= thirtyDaysAgo);
            var totalMembersBefore = stats.TotalMembers - newMembersCount;
            stats.GrowthRate = totalMembersBefore > 0 ? (double)newMembersCount / totalMembersBefore * 100 : 0;

            // Activity Logs
            var activityLogs = new List<ActivityPointDto>();

            // 1. New Members
            var memberJoins = await _userClubs
                .Where(uc => uc.ClubId == clubId && uc.JoinDate >= oneYearAgo)
                .Select(uc => new ActivityPointDto
                {
                    Date = uc.JoinDate.Date,
                    Count = 1,
                    Type = "MemberJoined",
                    Description = "New member joined"
                })
                .ToListAsync();
            activityLogs.AddRange(memberJoins);

            // 2. Events Created (Audit field 'Created' from AuditableBaseEntity)
            var eventsCreated = await _dbContext.Events
                .Where(e => e.ClubId == clubId && e.Created >= oneYearAgo)
                .Select(e => new ActivityPointDto
                {
                    Date = e.Created.Date,
                    Count = 2, // weighted
                    Type = "EventCreated",
                    Description = $"Event created: {e.Title}"
                })
                .ToListAsync();
            activityLogs.AddRange(eventsCreated);

            // 3. Attendance
            var attendances = await _dbContext.EventAttendees
                .Include(ea => ea.Event)
                .Where(ea => ea.Event.ClubId == clubId && ea.Created >= oneYearAgo && ea.Status == "Attended")
                .Select(ea => new ActivityPointDto
                {
                    Date = ea.Created.Date,
                    Count = 1,
                    Type = "Attendance",
                    Description = $"Member checked into {ea.Event.Title}"
                })
                .ToListAsync();
            activityLogs.AddRange(attendances);

            stats.ActivityLogs = activityLogs
                .GroupBy(a => a.Date)
                .Select(g => new ActivityPointDto
                {
                    Date = g.Key,
                    Count = g.Sum(x => x.Count),
                    Type = "Mixed", // Simplified for heatmap
                    Description = string.Join(", ", g.Select(x => x.Description).Distinct().Take(3))
                })
                .OrderBy(a => a.Date)
                .ToList();

            stats.TotalActivityPoints = stats.ActivityLogs.Sum(a => a.Count);
            return stats;
        }

        public async Task<IReadOnlyList<Club>> GetPresidentClubsAsync(string userId)
        {
            return await _userClubs
                .Include(uc => uc.Club)
                .Include(uc => uc.Role)
                .Where(uc => uc.UserId == userId
                    && uc.Role != null
                    && uc.Role.Name == "President")
                .Select(uc => uc.Club)
                .ToListAsync();
        }

        public async Task<bool> IsPresidentOfClubAsync(int clubId, string userId)
        {
            return await _userClubs
                .Include(uc => uc.Role)
                .AnyAsync(uc => uc.ClubId == clubId
                    && uc.UserId == userId
                    && uc.Role != null
                    && uc.Role.Name == "President");
        }

        public async Task<bool> HasAuthorityInClubAsync(int clubId, string userId)
        {
            return await _userClubs
                .Include(uc => uc.Role)
                .AnyAsync(uc => uc.ClubId == clubId
                    && uc.UserId == userId
                    && uc.IsActive
                    && uc.Role != null
                    && uc.Role.Name != "Active Member");
        }

        public async Task<bool> HasPrivilegeInClubAsync(int clubId, string userId, string privilegeName)
        {
            var userClub = await _userClubs
                .Include(uc => uc.Role)
                .ThenInclude(r => r.RolePrivileges)
                .ThenInclude(rp => rp.Privilege)
                .FirstOrDefaultAsync(uc => uc.ClubId == clubId && uc.UserId == userId && uc.IsActive);

            if (userClub == null || userClub.Role == null) return false;

            // President has all privileges
            if (userClub.Role.Name == "President") return true;

            return userClub.Role.RolePrivileges.Any(rp => rp.Privilege.Name == privilegeName);
        }
    }
}
