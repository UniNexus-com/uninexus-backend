using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;

namespace CleanArchitecture.Core.Features.Events.Common
{
    internal static class EventManagementPermissions
    {
        /// <summary>Üniversite etkinliğinde (host kulüp yok) mevcut davranış: ek kulüp yetkisi gerekmez.</summary>
        internal static async Task EnsureCanManageEventAsync(
            Event eventEntity,
            string userId,
            IClubRepositoryAsync clubRepository)
        {
            var hosts = eventEntity.EventClubs;
            if (hosts == null || !hosts.Any())
                return;

            foreach (var link in hosts)
            {
                if (await clubRepository.HasPrivilegeInClubAsync(link.ClubId, userId, "Manage Events"))
                    return;
            }

            throw new ApiException("You do not have permission to manage this event.");
        }
    }
}
