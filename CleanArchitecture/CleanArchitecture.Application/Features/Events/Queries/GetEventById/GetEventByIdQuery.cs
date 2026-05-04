using AutoMapper;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using RolesEnum = CleanArchitecture.Core.Enums.Roles;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Queries.GetEventById
{
    public class GetEventByIdQuery : IRequest<Response<EventViewModel>>
    {
        public int Id { get; set; }
    }

    public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, Response<EventViewModel>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetEventByIdQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IAuthenticatedUserService authenticatedUserService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _authenticatedUserService = authenticatedUserService;
            _userManager = userManager;
        }

        public async Task<Response<EventViewModel>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            var eventItem = await _context.Events
                .Include(e => e.EventClubs).ThenInclude(ec => ec.Club)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (eventItem == null) throw new ApiException($"Event Not Found.");

            // Visibility kontrolü — kullanıcının erişim yetkisi yoksa "bulunamadı" dön
            if (!await CanUserSeeAsync(eventItem, cancellationToken))
                throw new ApiException($"Event Not Found.");

            var eventViewModel = _mapper.Map<EventViewModel>(eventItem);

            var userId = _authenticatedUserService.UserId;
            if (!string.IsNullOrEmpty(userId))
            {
                eventViewModel.IsRegistered = await _context.EventAttendees
                    .AnyAsync(a => a.UserId == userId && a.EventId == eventItem.Id, cancellationToken);
            }

            return new Response<EventViewModel>(eventViewModel);
        }

        private async Task<bool> CanUserSeeAsync(Event eventItem, CancellationToken cancellationToken)
        {
            var visibility = string.IsNullOrEmpty(eventItem.Visibility) ? EventVisibility.Public : eventItem.Visibility;
            if (visibility == EventVisibility.Public) return true;

            var userId = _authenticatedUserService.UserId;
            if (string.IsNullOrEmpty(userId)) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(RolesEnum.SKS_ADMIN.ToString())) return true;
            }

            var hostClubIds = eventItem.EventClubs.Select(ec => ec.ClubId).ToList();
            if (hostClubIds.Count == 0) return true; // Üniversite etkinliği — host yok

            if (visibility == EventVisibility.MembersOnly)
            {
                return await _context.UserClubs.AnyAsync(
                    uc => uc.UserId == userId && uc.IsActive && hostClubIds.Contains(uc.ClubId),
                    cancellationToken);
            }

            if (visibility == EventVisibility.Private)
            {
                return await _context.UserClubs
                    .Where(uc => uc.UserId == userId && uc.IsActive && hostClubIds.Contains(uc.ClubId))
                    .Join(_context.ClubRolePrivileges, uc => uc.ClubRoleId, crp => crp.ClubRoleId, (uc, crp) => crp.PrivilegeId)
                    .Join(_context.ClubPrivileges, pid => pid, cp => cp.Id, (pid, cp) => cp.Name)
                    .AnyAsync(name => name == "Manage Events", cancellationToken);
            }

            return true;
        }
    }
}
