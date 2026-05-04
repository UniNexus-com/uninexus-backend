using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Interfaces;
using RolesEnum = CleanArchitecture.Core.Enums.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Core.Features.Events.Common
{
    /// <summary>
    /// Sunucu tarafı sızıntı önleyici visibility filtresi.
    /// Mobil/Web istemciler kullanıcının göremeyeceği etkinlikleri hiç almasın diye
    /// liste sorgularında çağrılır.
    /// </summary>
    internal static class EventVisibilityFilter
    {
        /// <summary>SKS_ADMIN ise her şeyi görür; aksi halde Public + üye olduğu kulüplerin etkinlikleri.</summary>
        internal static async Task<IQueryable<Event>> ApplyAsync(
            IQueryable<Event> query,
            IApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            string userId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
                return query.Where(e => e.Visibility == null || e.Visibility == "" || e.Visibility == EventVisibility.Public);

            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Contains(RolesEnum.SKS_ADMIN.ToString()))
                    return query;
            }

            var memberClubIds = await context.UserClubs
                .Where(uc => uc.UserId == userId && uc.IsActive)
                .Select(uc => uc.ClubId)
                .ToListAsync(cancellationToken);

            var manageClubIds = await context.UserClubs
                .Where(uc => uc.UserId == userId && uc.IsActive)
                .Join(context.ClubRolePrivileges,
                      uc => uc.ClubRoleId,
                      crp => crp.ClubRoleId,
                      (uc, crp) => new { uc.ClubId, crp.PrivilegeId })
                .Join(context.ClubPrivileges,
                      x => x.PrivilegeId,
                      cp => cp.Id,
                      (x, cp) => new { x.ClubId, cp.Name })
                .Where(x => x.Name == "Manage Events")
                .Select(x => x.ClubId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return query.Where(e =>
                e.Visibility == null || e.Visibility == "" || e.Visibility == EventVisibility.Public
                || (e.Visibility == EventVisibility.MembersOnly
                    && e.EventClubs.Any(ec => memberClubIds.Contains(ec.ClubId)))
                || (e.Visibility == EventVisibility.Private
                    && e.EventClubs.Any(ec => manageClubIds.Contains(ec.ClubId))));
        }
    }
}
