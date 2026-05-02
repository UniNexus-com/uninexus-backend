using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Seeds
{
    public static class DefaultClubsAndEvents
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Clubs.AnyAsync() || await context.Events.AnyAsync()) return;

            var now = DateTime.UtcNow;

            // 1. Seed Privileges
            var privileges = new List<ClubPrivilege>
            {
                new ClubPrivilege { Name = "Manage Members", Description = "Can approve/reject join requests and manage roles" },
                new ClubPrivilege { Name = "Manage Events", Description = "Can create and edit club events" },
                new ClubPrivilege { Name = "Manage Finances", Description = "Can see and manage club finances" },
                new ClubPrivilege { Name = "Manage Assets", Description = "Can manage club inventory" },
                new ClubPrivilege { Name = "View Reports", Description = "Can view club participation and financial reports" },
                new ClubPrivilege { Name = "Manage Roles", Description = "Can create custom roles and assign them to members" },
                new ClubPrivilege { Name = "Send Announcements", Description = "Can send club-wide messages and notifications" }
            };

            await context.AddRangeAsync(privileges);
            await context.SaveChangesAsync();

            // 2. Seed System Roles
            var systemRoles = new List<ClubRole>
            {
                new ClubRole { Name = "President", IsSystemRole = true, Created = now, CreatedBy = "seed" },
                new ClubRole { Name = "Vice President", IsSystemRole = true, Created = now, CreatedBy = "seed" },
                new ClubRole { Name = "Treasurer", IsSystemRole = true, Created = now, CreatedBy = "seed" },
                new ClubRole { Name = "Secretary", IsSystemRole = true, Created = now, CreatedBy = "seed" },
                new ClubRole { Name = "Active Member", IsSystemRole = true, Created = now, CreatedBy = "seed" },
                new ClubRole { Name = "Inventory Manager", IsSystemRole = true, Created = now, CreatedBy = "seed" },
                new ClubRole { Name = "Communications Officer", IsSystemRole = true, Created = now, CreatedBy = "seed" }
            };

            await context.ClubRoles.AddRangeAsync(systemRoles);
            await context.SaveChangesAsync();

            // 3. Link Roles to Privileges
            var rolePrivileges = new List<ClubRolePrivilege>();

            // President gets everything
            foreach (var p in privileges)
                rolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = systemRoles[0].Id, PrivilegeId = p.Id });

            // Vice President gets Members and Events
            rolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = systemRoles[1].Id, PrivilegeId = privileges[0].Id });
            rolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = systemRoles[1].Id, PrivilegeId = privileges[1].Id });

            // Treasurer gets Budget and Assets
            rolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = systemRoles[2].Id, PrivilegeId = privileges[2].Id });
            rolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = systemRoles[2].Id, PrivilegeId = privileges[3].Id });

            // Secretary gets Reports and Events
            rolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = systemRoles[3].Id, PrivilegeId = privileges[1].Id });
            rolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = systemRoles[3].Id, PrivilegeId = privileges[4].Id });

            // Inventory Manager gets Manage Assets
            rolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = systemRoles[5].Id, PrivilegeId = privileges[3].Id });

            // Communications Officer gets Send Announcements
            rolePrivileges.Add(new ClubRolePrivilege { ClubRoleId = systemRoles[6].Id, PrivilegeId = privileges[6].Id });

            await context.AddRangeAsync(rolePrivileges);
            await context.SaveChangesAsync();

            // 4. Seed Clubs
            var clubs = new List<Club>
            {
                new Club
                {
                    Name = "Yazılım ve Teknoloji Kulübü",
                    Description = "Yazılım geliştirme, yapay zeka ve teknoloji alanlarında projeler üreten öğrenci topluluğu.",
                    LogoUrl = null,
                    IsActive = true,
                    Created = now,
                    CreatedBy = "seed"
                },
                new Club
                {
                    Name = "Müzik Kulübü",
                    Description = "Her türden müziği seven ve icra eden öğrencilerin bir araya geldiği kulüp.",
                    LogoUrl = null,
                    IsActive = true,
                    Created = now,
                    CreatedBy = "seed"
                },
                new Club
                {
                    Name = "Fotoğrafçılık Kulübü",
                    Description = "Fotoğraf sanatını keşfeden ve paylaşan öğrenci topluluğu.",
                    LogoUrl = null,
                    IsActive = true,
                    Created = now,
                    CreatedBy = "seed"
                },
                new Club
                {
                    Name = "Girişimcilik ve İnovasyon Kulübü",
                    Description = "Startup kültürünü kampüse taşıyan, iş fikirleri geliştiren öğrenci kulübü.",
                    LogoUrl = null,
                    IsActive = true,
                    Created = now,
                    CreatedBy = "seed"
                },
                new Club
                {
                    Name = "Satranç Kulübü",
                    Description = "Satranç severler için turnuvalar ve eğitim etkinlikleri düzenleyen kulüp.",
                    LogoUrl = null,
                    IsActive = false,
                    Created = now,
                    CreatedBy = "seed"
                },
            };

            await context.Clubs.AddRangeAsync(clubs);
            await context.SaveChangesAsync();

            // 5. Seed Events
            var events = new List<Event>
            {
                // Yazılım ve Teknoloji Kulübü
                new Event
                {
                    Title = "Hackathon 2026",
                    Description = "24 saatlik yazılım maratonu. Takımlar gerçek dünya problemleri için çözüm geliştirecek.",
                    StartDate = now.AddDays(15),
                    EndDate = now.AddDays(16),
                    Location = "Mühendislik Fakültesi B Blok",
                    IsActive = true,
                    ClubId = clubs[0].Id,
                    Created = now,
                    CreatedBy = "seed"
                },
                new Event
                {
                    Title = "Yapay Zeka ile Tanışın",
                    Description = "Makine öğrenmesi ve yapay zekanın temellerini anlatan başlangıç seviyesi workshop.",
                    StartDate = now.AddDays(5),
                    EndDate = now.AddDays(5).AddHours(3),
                    Location = "Bilgisayar Lab 3",
                    IsActive = true,
                    ClubId = clubs[0].Id,
                    Created = now,
                    CreatedBy = "seed"
                },
                new Event
                {
                    Title = "Web Geliştirme Bootcamp",
                    Description = "React ve .NET ile modern web uygulamaları geliştirme eğitimi. 3 haftalık yoğun program.",
                    StartDate = now.AddDays(-10),
                    EndDate = now.AddDays(11),
                    Location = "Online + Kampüs Karma",
                    IsActive = true,
                    ClubId = clubs[0].Id,
                    Created = now,
                    CreatedBy = "seed"
                },

                // Müzik Kulübü
                new Event
                {
                    Title = "Bahar Konseri",
                    Description = "Öğrenci müzisyenlerden oluşan karma konser. Akustik, caz ve pop repertuvar.",
                    StartDate = now.AddDays(20),
                    EndDate = now.AddDays(20).AddHours(4),
                    Location = "Kültür Merkezi Ana Salon",
                    IsActive = true,
                    ClubId = clubs[1].Id,
                    Created = now,
                    CreatedBy = "seed"
                },
                new Event
                {
                    Title = "Gitar Atölyesi",
                    Description = "Başlangıç ve orta seviye gitar öğrencileri için uygulamalı atölye.",
                    StartDate = now.AddDays(3),
                    EndDate = now.AddDays(3).AddHours(2),
                    Location = "Müzik Odası 101",
                    IsActive = true,
                    ClubId = clubs[1].Id,
                    Created = now,
                    CreatedBy = "seed"
                },

                // Fotoğrafçılık Kulübü
                new Event
                {
                    Title = "Kampüs Fotoğraf Yarışması",
                    Description = "Kampüs köşelerini en iyi yansıtan karede büyük ödül. Başvurular açık.",
                    StartDate = now.AddDays(1),
                    EndDate = now.AddDays(21),
                    Location = "Kampüs Geneli",
                    IsActive = true,
                    ClubId = clubs[2].Id,
                    Created = now,
                    CreatedBy = "seed"
                },
                new Event
                {
                    Title = "Karanlık Oda Eğitimi",
                    Description = "Analog fotoğraf baskı tekniklerini öğrenmek isteyenler için sınırlı kontenjan.",
                    StartDate = now.AddDays(-5),
                    EndDate = now.AddDays(-5).AddHours(3),
                    Location = "Güzel Sanatlar Fakültesi Foto Lab",
                    IsActive = false,
                    ClubId = clubs[2].Id,
                    Created = now,
                    CreatedBy = "seed"
                },

                // Girişimcilik Kulübü
                new Event
                {
                    Title = "Demo Day 2026",
                    Description = "Öğrenci startup'larının yatırımcılara sunum yaptığı yıllık etkinlik.",
                    StartDate = now.AddDays(30),
                    EndDate = now.AddDays(30).AddHours(5),
                    Location = "Rektörlük Konferans Salonu",
                    IsActive = true,
                    ClubId = clubs[3].Id,
                    Created = now,
                    CreatedBy = "seed"
                },
                new Event
                {
                    Title = "Mentorluk Buluşması",
                    Description = "Sektör profesyonelleriyle birebir mentorluk seansları. Kayıt zorunlu.",
                    StartDate = now.AddDays(8),
                    EndDate = now.AddDays(8).AddHours(6),
                    Location = "Girişimcilik Merkezi",
                    IsActive = true,
                    ClubId = clubs[3].Id,
                    Created = now,
                    CreatedBy = "seed"
                },

                // Kulüpsüz etkinlik
                new Event
                {
                    Title = "Üniversite Tanıtım Günleri",
                    Description = "Yeni öğrenciler için kampüs turu ve bölüm tanıtımları.",
                    StartDate = now.AddDays(-2),
                    EndDate = now.AddDays(-1),
                    Location = "Ana Kampüs",
                    IsActive = false,
                    ClubId = null,
                    Created = now,
                    CreatedBy = "seed"
                },
            };

            await context.Events.AddRangeAsync(events);
            await context.SaveChangesAsync();

            // 6. Seed Default Channels for each club
            var presidentRole = systemRoles[0]; // President
            var vpRole = systemRoles[1];         // Vice President

            foreach (var club in clubs)
            {
                var generalChannel = new ClubChannel
                {
                    ClubId = club.Id,
                    Name = "general",
                    Description = "General discussion for all members",
                    IsDefault = true,
                    SortOrder = 0,
                    Created = now,
                    CreatedBy = "seed"
                };
                var announcementsChannel = new ClubChannel
                {
                    ClubId = club.Id,
                    Name = "announcements",
                    Description = "Official announcements from club leaders",
                    IsDefault = true,
                    SortOrder = 1,
                    Created = now,
                    CreatedBy = "seed"
                };
                var eventsChannel = new ClubChannel
                {
                    ClubId = club.Id,
                    Name = "events",
                    Description = "Event coordination and updates",
                    IsDefault = true,
                    SortOrder = 2,
                    Created = now,
                    CreatedBy = "seed"
                };
                var qaChannel = new ClubChannel
                {
                    ClubId = club.Id,
                    Name = "q-and-a",
                    Description = "Questions and answers for all members",
                    IsDefault = true,
                    SortOrder = 3,
                    Created = now,
                    CreatedBy = "seed"
                };

                await context.ClubChannels.AddRangeAsync(generalChannel, announcementsChannel, eventsChannel, qaChannel);
                await context.SaveChangesAsync();

                // #general and #q-and-a: no write roles = everyone can write
                // #announcements and #events: only President and VP can write
                var channelWriteRoles = new List<ClubChannelWriteRole>
                {
                    new ClubChannelWriteRole { ChannelId = announcementsChannel.Id, ClubRoleId = presidentRole.Id },
                    new ClubChannelWriteRole { ChannelId = announcementsChannel.Id, ClubRoleId = vpRole.Id },
                    new ClubChannelWriteRole { ChannelId = eventsChannel.Id, ClubRoleId = presidentRole.Id },
                    new ClubChannelWriteRole { ChannelId = eventsChannel.Id, ClubRoleId = vpRole.Id },
                };

                await context.AddRangeAsync(channelWriteRoles);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Seeds default channels for all clubs that don't have any channels yet.
        /// Runs independently so channels get created for existing clubs.
        /// </summary>
        public static async Task SeedChannelsAsync(ApplicationDbContext context)
        {
            if (await context.ClubChannels.AnyAsync()) return;

            var clubs = await context.Clubs.ToListAsync();
            if (!clubs.Any()) return;

            var presidentRole = await context.ClubRoles.FirstOrDefaultAsync(r => r.Name == "President");
            var vpRole = await context.ClubRoles.FirstOrDefaultAsync(r => r.Name == "Vice President");
            var now = DateTime.UtcNow;

            foreach (var club in clubs)
            {
                var generalChannel = new ClubChannel { ClubId = club.Id, Name = "general", Description = "General discussion for all members", IsDefault = true, SortOrder = 0, Created = now, CreatedBy = "seed" };
                var announcementsChannel = new ClubChannel { ClubId = club.Id, Name = "announcements", Description = "Official announcements from club leaders", IsDefault = true, SortOrder = 1, Created = now, CreatedBy = "seed" };
                var eventsChannel = new ClubChannel { ClubId = club.Id, Name = "events", Description = "Event coordination and updates", IsDefault = true, SortOrder = 2, Created = now, CreatedBy = "seed" };
                var qaChannel = new ClubChannel { ClubId = club.Id, Name = "q-and-a", Description = "Questions and answers for all members", IsDefault = true, SortOrder = 3, Created = now, CreatedBy = "seed" };

                await context.ClubChannels.AddRangeAsync(generalChannel, announcementsChannel, eventsChannel, qaChannel);
                await context.SaveChangesAsync();

                if (presidentRole != null && vpRole != null)
                {
                    var writeRoles = new List<ClubChannelWriteRole>
                    {
                        new ClubChannelWriteRole { ChannelId = announcementsChannel.Id, ClubRoleId = presidentRole.Id },
                        new ClubChannelWriteRole { ChannelId = announcementsChannel.Id, ClubRoleId = vpRole.Id },
                        new ClubChannelWriteRole { ChannelId = eventsChannel.Id, ClubRoleId = presidentRole.Id },
                        new ClubChannelWriteRole { ChannelId = eventsChannel.Id, ClubRoleId = vpRole.Id },
                    };
                    await context.AddRangeAsync(writeRoles);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
