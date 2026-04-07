using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Seeds
{
    public static class DefaultClubLeaders
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            // Seed Club Leader User
            var defaultUser = new ApplicationUser
            {
                UserName = "clubleader",
                Email = "leader@akdeniz.edu.tr",
                FullName = "Kulüp Lideri",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            if (userManager.Users.All(u => u.UserName != defaultUser.UserName))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "Leader123!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.CLUB_LEADER.ToString());
                }
                else
                {
                    defaultUser = user;
                }
            }
            else
            {
                defaultUser = await userManager.FindByNameAsync(defaultUser.UserName);
            }

            // Link to Yazılım Kulübü as President
            var club = await dbContext.Clubs.FirstOrDefaultAsync(c => c.Name == "Yazılım ve Teknoloji Kulübü");
            var presidentRole = await dbContext.ClubRoles.FirstOrDefaultAsync(r => r.Name == "President" && r.IsSystemRole);

            if (club != null && presidentRole != null)
            {
                var userClubExists = await dbContext.UserClubs
                    .AnyAsync(uc => uc.UserId == defaultUser.Id && uc.ClubId == club.Id);

                if (!userClubExists)
                {
                    var userClub = new UserClub
                    {
                        UserId = defaultUser.Id,
                        ClubId = club.Id,
                        ClubRoleId = presidentRole.Id,
                        JoinDate = DateTime.UtcNow,
                        IsActive = true,
                        Created = DateTime.UtcNow,
                        CreatedBy = "seed"
                    };

                    await dbContext.UserClubs.AddAsync(userClub);
                    await dbContext.SaveChangesAsync();
                }
            }

            // Also seed some members and join requests for this club to test the panel
            await SeedMembersAndRequests(userManager, dbContext, club);
        }

        private static async Task SeedMembersAndRequests(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, Club club)
        {
            if (club == null) return;

            // 1. Seed some regular students
            var students = new List<ApplicationUser>
            {
                new ApplicationUser { UserName = "student1", Email = "student1@akdeniz.edu.tr", FullName = "Ali Yılmaz", EmailConfirmed = true },
                new ApplicationUser { UserName = "student2", Email = "student2@akdeniz.edu.tr", FullName = "Ayşe Demir", EmailConfirmed = true },
                new ApplicationUser { UserName = "student3", Email = "student3@akdeniz.edu.tr", FullName = "Mehmet Kaya", EmailConfirmed = true },
                new ApplicationUser { UserName = "student4", Email = "student4@akdeniz.edu.tr", FullName = "Fatma Çelik", EmailConfirmed = true }
            };

            foreach (var student in students)
            {
                if (userManager.Users.All(u => u.UserName != student.UserName))
                {
                    await userManager.CreateAsync(student, "Student123!");
                    await userManager.AddToRoleAsync(student, Roles.STUDENT.ToString());
                }
            }

            // Reload students with IDs
            var student1 = await userManager.FindByNameAsync("student1");
            var student2 = await userManager.FindByNameAsync("student2");
            var student3 = await userManager.FindByNameAsync("student3");
            var student4 = await userManager.FindByNameAsync("student4");

            // 2. Add as active members
            var memberRole = await dbContext.ClubRoles.FirstOrDefaultAsync(r => r.Name == "Active Member");
            var vicePresidentRole = await dbContext.ClubRoles.FirstOrDefaultAsync(r => r.Name == "Vice President");
            var treasurerRole = await dbContext.ClubRoles.FirstOrDefaultAsync(r => r.Name == "Treasurer");

            if (memberRole != null)
            {
                var membersToInvite = new List<(ApplicationUser User, int RoleId)>
                {
                    (student1, memberRole.Id),
                    (student2, memberRole.Id),
                    (student3, vicePresidentRole?.Id ?? memberRole.Id),
                    (student4, treasurerRole?.Id ?? memberRole.Id)
                };

                foreach (var m in membersToInvite)
                {
                    if (!await dbContext.UserClubs.AnyAsync(uc => uc.UserId == m.User.Id && uc.ClubId == club.Id))
                    {
                        await dbContext.UserClubs.AddAsync(new UserClub 
                        { 
                            UserId = m.User.Id, 
                            ClubId = club.Id, 
                            ClubRoleId = m.RoleId, 
                            JoinDate = DateTime.UtcNow.AddMonths(-new Random().Next(1, 6)), 
                            IsActive = true,
                            Created = DateTime.UtcNow,
                            CreatedBy = "seed"
                        });
                    }
                }

                // Add default student as 6th member
                var defaultStudent = await userManager.FindByNameAsync("defaultstudent");
                if (defaultStudent != null && !await dbContext.UserClubs.AnyAsync(uc => uc.UserId == defaultStudent.Id && uc.ClubId == club.Id))
                {
                    await dbContext.UserClubs.AddAsync(new UserClub 
                    { 
                        UserId = defaultStudent.Id, 
                        ClubId = club.Id, 
                        ClubRoleId = memberRole.Id, 
                        JoinDate = DateTime.UtcNow.AddDays(-10), 
                        IsActive = true,
                        Created = DateTime.UtcNow,
                        CreatedBy = "seed"
                    });
                }
            }

            // 3. Add new students as pending join requests (to keep testing requests functionality)
            var requester1 = new ApplicationUser { UserName = "requester1", Email = "req1@akdeniz.edu.tr", FullName = "Caner Şen", EmailConfirmed = true };
            var requester2 = new ApplicationUser { UserName = "requester2", Email = "req2@akdeniz.edu.tr", FullName = "Elif Sönmez", EmailConfirmed = true };

            foreach (var req in new[] { requester1, requester2 })
            {
                if (userManager.Users.All(u => u.UserName != req.UserName))
                {
                    await userManager.CreateAsync(req, "Student123!");
                    await userManager.AddToRoleAsync(req, Roles.STUDENT.ToString());
                }
                
                var user = await userManager.FindByNameAsync(req.UserName);
                if (!await dbContext.ClubJoinRequests.AnyAsync(cjr => cjr.UserId == user.Id && cjr.ClubId == club.Id))
                {
                    await dbContext.ClubJoinRequests.AddAsync(new ClubJoinRequest
                    {
                        UserId = user.Id,
                        ClubId = club.Id,
                        Status = ClubJoinStatus.Pending,
                        Created = DateTime.UtcNow,
                        CreatedBy = user.UserName
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
