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

            // 2. Add some as active members
            var memberRole = await dbContext.ClubRoles.FirstOrDefaultAsync(r => r.Name == "Active Member");
            if (memberRole != null)
            {
                if (!await dbContext.UserClubs.AnyAsync(uc => uc.UserId == student1.Id && uc.ClubId == club.Id))
                {
                    await dbContext.UserClubs.AddAsync(new UserClub 
                    { 
                        UserId = student1.Id, 
                        ClubId = club.Id, 
                        ClubRoleId = memberRole.Id, 
                        JoinDate = DateTime.UtcNow.AddMonths(-1), 
                        IsActive = true,
                        Created = DateTime.UtcNow,
                        CreatedBy = "seed"
                    });
                }
                if (!await dbContext.UserClubs.AnyAsync(uc => uc.UserId == student2.Id && uc.ClubId == club.Id))
                {
                    await dbContext.UserClubs.AddAsync(new UserClub 
                    { 
                        UserId = student2.Id, 
                        ClubId = club.Id, 
                        ClubRoleId = memberRole.Id, 
                        JoinDate = DateTime.UtcNow.AddMonths(-2), 
                        IsActive = true,
                        Created = DateTime.UtcNow,
                        CreatedBy = "seed"
                    });
                }
            }

            // 3. Add some as pending join requests
            if (!await dbContext.ClubJoinRequests.AnyAsync(cjr => cjr.UserId == student3.Id && cjr.ClubId == club.Id))
            {
                await dbContext.ClubJoinRequests.AddAsync(new ClubJoinRequest
                {
                    UserId = student3.Id,
                    ClubId = club.Id,
                    Status = 0, // Pending
                    Created = DateTime.UtcNow,
                    CreatedBy = student3.UserName
                });
            }
            if (!await dbContext.ClubJoinRequests.AnyAsync(cjr => cjr.UserId == student4.Id && cjr.ClubId == club.Id))
            {
                await dbContext.ClubJoinRequests.AddAsync(new ClubJoinRequest
                {
                    UserId = student4.Id,
                    ClubId = club.Id,
                    Status = 0, // Pending
                    Created = DateTime.UtcNow,
                    CreatedBy = student4.UserName
                });
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
