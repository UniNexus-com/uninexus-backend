using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Account;

namespace CleanArchitecture.Infrastructure.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IDateTimeService _dateTime;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Club> Clubs { get; set; }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IDateTimeService dateTime = null,
            IAuthenticatedUserService authenticatedUser = null) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_dateTime != null)
            {

                foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.Entity.Created = _dateTime.NowUtc;
                            entry.Entity.CreatedBy = _authenticatedUser.UserId;
                            break;
                        case EntityState.Modified:
                            entry.Entity.LastModified = _dateTime.NowUtc;
                            entry.Entity.LastModifiedBy = _authenticatedUser.UserId;
                            break;
                    }
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // -- Application User -----
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "users");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.StudentNumber)
                    .HasMaxLength(11)
                    .IsRequired(false);
                entity.HasIndex(e => e.StudentNumber).IsUnique();

                entity.HasMany(e => e.RefreshTokens)
                    .WithOne()
                    .HasForeignKey(rt => rt.ApplicationUserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // -- Refresh Token -----
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable(name: "refresh_tokens");

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.TokenHash)
                    .HasColumnName("token_hash")
                    .IsRequired();

                entity.Property(e => e.ApplicationUserId).
                    HasColumnName("application_user_id").
                    IsRequired();
                entity.Property(e => e.Platform)
                    .HasColumnName("platform");

                entity.Property(e => e.ExpiresAt)
                    .HasColumnName("expires_at")
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();

                entity.Property(e => e.CreatedByIp)
                    .HasColumnName("created_by_ip")
                    .IsRequired();

                entity.Property(e => e.RevokedAt)
                .HasColumnName("revoked_at");

                entity.Property(e => e.RevokedByIp)
                .HasColumnName("revoked_by_ip");

                entity.Property(e => e.ReplacedByToken)
                .HasColumnName("replaced_by_token");
            });

            // -- Clubs -----
            builder.Entity<Club>(entity =>
            {
                entity.ToTable(name: "clubs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.LogoUrl).HasColumnName("logo_url").HasMaxLength(500);
                entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.Created).HasColumnName("created");
                entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
                entity.Property(e => e.LastModified).HasColumnName("last_modified");
            });

            // -- Events -----
            builder.Entity<Event>(entity =>
            {
                entity.ToTable(name: "events");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.StartDate).HasColumnName("start_date").IsRequired();
                entity.Property(e => e.EndDate).HasColumnName("end_date").IsRequired();
                entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(200);
                entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
                entity.Property(e => e.ClubId).HasColumnName("club_id");

                entity.HasOne(e => e.Club)
                    .WithMany(c => c.Events)
                    .HasForeignKey(e => e.ClubId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });


            // -- Identity Tables -----
            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "roles");
            });
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("user_roles");
            });
            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("user_claims");
            });
            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("user_logins");
            });
            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("role_claims");
            });
            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("user_tokens");
            });
        }
    }
}
