using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Account;

namespace CleanArchitecture.Infrastructure.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        private readonly IDateTimeService _dateTime;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<ClubRole> ClubRoles { get; set; }
        public DbSet<ClubPrivilege> ClubPrivileges { get; set; }
        public DbSet<ClubRolePrivilege> ClubRolePrivileges { get; set; }
        public DbSet<UserClub> UserClubs { get; set; }
        public DbSet<ClubJoinRequest> ClubJoinRequests { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<BudgetRequest> BudgetRequests { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

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

                entity.HasMany(e => e.UserClubs)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.JoinRequests)
                    .WithOne()
                    .HasForeignKey(jr => jr.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.EventAttendees)
                    .WithOne()
                    .HasForeignKey(ea => ea.UserId)
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
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("ACTIVE");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.Created).HasColumnName("created");
                entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
                entity.Property(e => e.LastModified).HasColumnName("last_modified");
                entity.Property(e => e.TotalBudget).HasColumnName("total_budget").HasColumnType("numeric(12,2)");
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
                entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
                entity.Property(e => e.Visibility).HasColumnName("visibility").HasMaxLength(50).HasDefaultValue("All Students");
                entity.Property(e => e.Capacity).HasColumnName("capacity");
                entity.Property(e => e.Requirements).HasColumnName("requirements");
                entity.Property(e => e.RequireApproval).HasColumnName("require_approval").HasDefaultValue(false);
                entity.Property(e => e.Tags).HasColumnName("tags").HasMaxLength(500);

                entity.HasOne(e => e.Club)
                    .WithMany(c => c.Events)
                    .HasForeignKey(e => e.ClubId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            
            // -- Club Roles -----
            builder.Entity<ClubRole>(entity =>
            {
                entity.ToTable(name: "club_roles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.ClubId).HasColumnName("club_id");
                entity.Property(e => e.IsSystemRole).HasColumnName("is_system_role").HasDefaultValue(false);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Color).HasColumnName("color").HasMaxLength(20);
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.Created).HasColumnName("created");
                entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.HasOne(e => e.Club)
                    .WithMany(c => c.CustomRoles)
                    .HasForeignKey(e => e.ClubId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // -- Club Privileges -----
            builder.Entity<ClubPrivilege>(entity =>
            {
                entity.ToTable(name: "club_privileges");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(250);
            });

            // -- Club Role Privileges -----
            builder.Entity<ClubRolePrivilege>(entity =>
            {
                entity.ToTable(name: "club_role_privileges");
                entity.HasKey(e => new { e.ClubRoleId, e.PrivilegeId });
                entity.Property(e => e.ClubRoleId).HasColumnName("club_role_id");
                entity.Property(e => e.PrivilegeId).HasColumnName("privilege_id");

                entity.HasOne(e => e.ClubRole)
                    .WithMany(r => r.RolePrivileges)
                    .HasForeignKey(e => e.ClubRoleId);

                entity.HasOne(e => e.Privilege)
                    .WithMany(p => p.RolePrivileges)
                    .HasForeignKey(e => e.PrivilegeId);
            });

            // -- User Clubs (Membership) -----
            builder.Entity<UserClub>(entity =>
            {
                entity.ToTable(name: "user_clubs");
                entity.HasKey(e => new { e.UserId, e.ClubId });
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ClubId).HasColumnName("club_id");
                entity.Property(e => e.ClubRoleId).HasColumnName("club_role_id");
                entity.Property(e => e.JoinDate).HasColumnName("join_date");
                entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(e => e.Club)
                    .WithMany(c => c.UserClubs)
                    .HasForeignKey(e => e.ClubId);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.UserClubs)
                    .HasForeignKey(e => e.ClubRoleId);
            });

            // -- Club Join Requests -----
            builder.Entity<ClubJoinRequest>(entity =>
            {
                entity.ToTable(name: "club_join_requests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.ClubId).HasColumnName("club_id").IsRequired();
                entity.Property(e => e.Status).HasColumnName("status").HasConversion<int>().IsRequired();
                entity.Property(e => e.ProcessedBy).HasColumnName("processed_by");
                entity.Property(e => e.ProcessedDate).HasColumnName("processed_date");

                entity.HasOne(e => e.Club)
                    .WithMany(c => c.JoinRequests)
                    .HasForeignKey(e => e.ClubId);
            });

            // -- Assets -----
            builder.Entity<Asset>(entity =>
            {
                entity.ToTable(name: "assets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(300);
                entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
                entity.Property(e => e.Condition).HasColumnName("condition").HasMaxLength(50);
                entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(200);
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("AVAILABLE");
                entity.Property(e => e.Value).HasColumnName("value").HasColumnType("numeric(12,2)");
                entity.Property(e => e.SerialNo).HasColumnName("serial_no").HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ClubId).HasColumnName("club_id");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.Created).HasColumnName("created");
                entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.HasOne(e => e.Club)
                    .WithMany(c => c.Assets)
                    .HasForeignKey(e => e.ClubId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // -- Budget Requests -----
            builder.Entity<BudgetRequest>(entity =>
            {
                entity.ToTable("budget_requests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
                entity.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("numeric(12,2)");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("PENDING");
                entity.Property(e => e.ClubId).HasColumnName("club_id");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.Created).HasColumnName("created");
                entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.HasOne(e => e.Club)
                    .WithMany(c => c.BudgetRequests)
                    .HasForeignKey(e => e.ClubId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // -- Event Attendees -----
            builder.Entity<EventAttendee>(entity =>
            {
                entity.ToTable(name: "event_attendees");
                entity.HasKey(e => new { e.UserId, e.EventId });
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.EventId).HasColumnName("event_id");
                entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);

                entity.HasOne(e => e.Event)
                    .WithMany()
                    .HasForeignKey(e => e.EventId);
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
