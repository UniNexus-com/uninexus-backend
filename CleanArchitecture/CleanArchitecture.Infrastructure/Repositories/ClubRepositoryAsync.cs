using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
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
                            StudentNumber = u.StudentNumber,
                            Status = u.Status.ToString(),
                            Joined = uc.JoinDate
                        };

            return await query.ToListAsync();
        }

        public async Task<(IReadOnlyList<ClubMemberDto> Data, int TotalCount)> GetClubMembersPagedAsync(
            int clubId, 
            int pageNumber, 
            int pageSize, 
            string searchValue,
            string sortColumn,
            string sortDirection,
            List<string> roleFilters,
            List<string> statusFilters)
        {
            var query = from uc in _userClubs
                        join u in _users on uc.UserId equals u.Id
                        join r in _dbContext.ClubRoles on uc.ClubRoleId equals r.Id into roles
                        from r in roles.DefaultIfEmpty()
                        where uc.ClubId == clubId
                        select new { uc, u, r };

            // Search
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower();
                query = query.Where(x => x.u.FullName.ToLower().Contains(searchValue) || x.u.Email.ToLower().Contains(searchValue) || x.u.StudentNumber.ToLower().Contains(searchValue));
            }

            // Role Filters
            if (roleFilters != null && roleFilters.Any())
            {
                query = query.Where(x => x.r != null && roleFilters.Contains(x.r.Name));
            }

            // Status Filters
            if (statusFilters != null && statusFilters.Any())
            {
                var statusEnums = statusFilters
                    .Select(s => System.Enum.TryParse<AccountStatus>(s, true, out var result) ? (AccountStatus?)result : null)
                    .Where(s => s.HasValue)
                    .Select(s => s.Value)
                    .ToList();

                if (statusEnums.Any())
                {
                    query = query.Where(x => statusEnums.Contains(x.u.Status));
                }
            }

            // Sorting (before projection to avoid ToString translation issues)
            if (!string.IsNullOrEmpty(sortColumn))
            {
                bool isAsc = sortDirection?.ToLower() == "asc";
                switch (sortColumn.ToLower())
                {
                    case "name":
                    case "fullname":
                        query = isAsc ? query.OrderBy(x => x.u.FullName) : query.OrderByDescending(x => x.u.FullName);
                        break;
                    case "role":
                        query = isAsc ? query.OrderBy(x => x.r != null ? x.r.Name : "Member") : query.OrderByDescending(x => x.r != null ? x.r.Name : "Member");
                        break;
                    case "studentnumber":
                        query = isAsc ? query.OrderBy(x => x.u.StudentNumber) : query.OrderByDescending(x => x.u.StudentNumber);
                        break;
                    case "status":
                        query = isAsc ? query.OrderBy(x => x.u.Status) : query.OrderByDescending(x => x.u.Status);
                        break;
                    case "joined":
                        query = isAsc ? query.OrderBy(x => x.uc.JoinDate) : query.OrderByDescending(x => x.uc.JoinDate);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.uc.JoinDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(x => x.uc.JoinDate);
            }

            var finalQuery = query
                        .Select(x => new ClubMemberDto
                        {
                            Id = x.u.Id,
                            Name = x.u.FullName,
                            Email = x.u.Email,
                            Role = x.r != null ? x.r.Name : "Member",
                            RoleId = x.uc.ClubRoleId,
                            RoleColor = x.r != null ? (x.r.Color ?? "#3b82f6") : "#94a3b8",
                            IsPresident = x.r != null && x.r.Name == "President",
                            StudentNumber = x.u.StudentNumber,
                            Status = x.u.Status == AccountStatus.Active ? "Active" : "Suspended",
                            Joined = x.uc.JoinDate
                        });

            var totalCount = await finalQuery.CountAsync();
            var data = await finalQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
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

        public async Task AddJoinRequestAsync(ClubJoinRequest joinRequest)
        {
            await _joinRequests.AddAsync(joinRequest);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<MemberDetailsDto> GetClubMemberDetailsAsync(int clubId, string userId)
        {
            var user = await _users.FindAsync(userId);
            if (user == null) return null;

            var userClub = await _userClubs
                .Include(uc => uc.Role)
                .FirstOrDefaultAsync(uc => uc.ClubId == clubId && uc.UserId == userId);

            if (userClub == null) return null;

            // Fetch activities (attendance history)
            var activities = await _dbContext.EventAttendees
                .Include(ea => ea.Event)
                .Where(ea => ea.UserId == userId && ea.Event.ClubId == clubId)
                .OrderByDescending(ea => ea.Event.StartDate)
                .Select(ea => new MemberActivityDto
                {
                    Title = ea.Event.Title,
                    Subtitle = ea.Status == "Attended" ? $"Checked in • {ea.Event.StartDate:dd MMM}" : $"{ea.Status} • {ea.Event.StartDate:dd MMM}",
                    Date = ea.Event.StartDate,
                    Type = "Attendance",
                    Status = ea.Status
                })
                .Take(5)
                .ToListAsync();

            // Calculate stats
            var totalAttended = await _dbContext.EventAttendees
                .CountAsync(ea => ea.UserId == userId && ea.Event.ClubId == clubId && ea.Status == "Attended");
            
            var totalRegistered = await _dbContext.EventAttendees
                .CountAsync(ea => ea.UserId == userId && ea.Event.ClubId == clubId);

            var reliability = totalRegistered > 0 ? (double)totalAttended / totalRegistered * 100 : 0;

            var details = new MemberDetailsDto
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                StudentNumber = user.StudentNumber,
                Role = userClub.Role?.Name ?? "Member",
                RoleColor = userClub.Role?.Color ?? (userClub.Role?.Name == "President" ? "#F5A623" : "#3b82f6"),
                IsPresident = userClub.Role?.Name == "President",
                Joined = userClub.JoinDate,
                Phone = user.PhoneNumber,
                Major = "Engineering", // still hardcoded but better than nothing for now
                Year = "3rd Year",
                Bio = "Member of UniNexus",
                
                EventsAttended = totalAttended,
                Reliability = reliability,
                ProjectsLed = 0, // Not tracked yet
                MemberTier = totalAttended > 10 ? "Gold" : (totalAttended > 5 ? "Silver" : "Bronze"),
                
                Activities = activities
            };

            return details;
        }
        public async Task<ClubHistoryDto> GetClubHistoryAsync(int clubId)
        {
            var members = await _userClubs
                .Where(uc => uc.ClubId == clubId)
                .OrderBy(uc => uc.JoinDate)
                .Select(uc => new { uc.JoinDate })
                .ToListAsync();

            var events = await _dbContext.Events
                .Where(e => e.ClubId == clubId && e.IsActive)
                .OrderBy(e => e.Created)
                .Select(e => new { e.Created })
                .ToListAsync();

            var budgets = await _dbContext.BudgetRequests
                .Where(b => b.ClubId == clubId && b.Status == "APPROVED")
                .OrderBy(b => b.Created)
                .Select(b => new { b.Created, b.Amount })
                .ToListAsync();

            var history = new ClubHistoryDto();
            
            var allDates = members.Select(m => m.JoinDate.Date)
                .Union(events.Select(e => e.Created.Date))
                .Union(budgets.Select(b => b.Created.Date))
                .OrderBy(d => d)
                .Distinct()
                .ToList();

            if (!allDates.Any()) return history;

            int currentMembers = 0;
            int currentEvents = 0;
            decimal currentBudget = 0;

            foreach (var date in allDates)
            {
                currentMembers += members.Count(m => m.JoinDate.Date == date);
                currentEvents += events.Count(e => e.Created.Date == date);
                currentBudget += budgets.Where(b => b.Created.Date == date).Sum(b => b.Amount);

                history.Points.Add(new HistoryPointDto
                {
                    Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                    MemberCount = currentMembers,
                    EventCount = currentEvents,
                    TotalBudgetUsed = currentBudget
                });
            }

            return history;
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

            // Calculate Growth Rate (Current Month Growth)
            // Each month's value includes previous dates (cumulative), so the difference 
            // between 'now' and 'end of last month' gives this month's growth.
            var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var newMembersThisMonth = await _userClubs.CountAsync(uc => uc.ClubId == clubId && uc.JoinDate >= thisMonthStart);
            
            stats.GrowthRate = (double)newMembersThisMonth;

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

        public async Task<ClubRole> GetSystemRoleByNameAsync(string roleName)
        {
            return await _clubRoles
                .FirstOrDefaultAsync(r => r.Name == roleName && r.IsSystemRole);
        }

        public async Task<ClubUserPermissionsDto> GetClubUserPermissionsAsync(int clubId, string userId)
        {
            var userClub = await _userClubs
                .Include(uc => uc.Club)
                .Include(uc => uc.Role)
                .ThenInclude(r => r.RolePrivileges)
                .ThenInclude(rp => rp.Privilege)
                .FirstOrDefaultAsync(uc => uc.ClubId == clubId && uc.UserId == userId && uc.IsActive);

            if (userClub == null || userClub.Role == null) return null;

            var permissions = new ClubUserPermissionsDto
            {
                ClubId = clubId,
                Status = userClub.Club.Status,
                Role = userClub.Role.Name,
                IsPresident = userClub.Role.Name == "President",
                Privileges = userClub.Role.Name == "President"
                    ? await _dbContext.ClubPrivileges.Select(p => p.Name).ToListAsync()
                    : userClub.Role.RolePrivileges.Select(rp => rp.Privilege.Name).ToList()
            };

            return permissions;
        }
        public async Task<bool> IsClubActiveAsync(int clubId)
        {
            var club = await _dbContext.Clubs.FindAsync(clubId);
            if (club == null) return false;

            return club.Status != "PENDING" && club.Status != "CLOSED";
        }
    }
}
