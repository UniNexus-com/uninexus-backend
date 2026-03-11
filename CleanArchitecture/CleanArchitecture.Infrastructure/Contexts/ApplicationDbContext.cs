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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTimeService dateTime = null, IAuthenticatedUserService authenticatedUser = null) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
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
            base.OnModelCreating(builder); // en başa taşındı

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
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable(name: "refresh_tokens");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Token).HasColumnName("token_hash").IsRequired();
                entity.Property(e => e.ApplicationUserId).HasColumnName("application_user_id").IsRequired();
                entity.Property(e => e.Platform).HasColumnName("platform");
                entity.Property(e => e.Expires).HasColumnName("expires_at").IsRequired();
                entity.Property(e => e.Created).HasColumnName("created_at").IsRequired();
                entity.Property(e => e.CreatedByIp).HasColumnName("created_by_ip").IsRequired();
                entity.Property(e => e.Revoked).HasColumnName("revoked_at");
                entity.Property(e => e.RevokedByIp).HasColumnName("revoked_by_ip");
                entity.Property(e => e.ReplacedByToken).HasColumnName("replaced_by_token");
            });
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
