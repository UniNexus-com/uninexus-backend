using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Seeds
{
    public static class DefaultClubsAndEvents
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Clubs.AnyAsync() || await context.Events.AnyAsync()) return;

            var now = DateTime.UtcNow;

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
        }
    }
}
