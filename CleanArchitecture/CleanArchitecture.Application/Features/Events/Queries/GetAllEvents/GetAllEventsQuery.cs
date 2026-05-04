using AutoMapper;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Events.Common;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Queries.GetAllEvents
{
    public class GetAllEventsQuery : IRequest<Response<IEnumerable<EventViewModel>>>
    {
        public int? ClubId { get; set; }
    }

    public class GetAllEventsQueryHandler : IRequestHandler<GetAllEventsQuery, Response<IEnumerable<EventViewModel>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetAllEventsQueryHandler(
            IApplicationDbContext context,
            IClubRepositoryAsync clubRepository,
            IMapper mapper,
            IAuthenticatedUserService authenticatedUserService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _clubRepository = clubRepository;
            _mapper = mapper;
            _authenticatedUserService = authenticatedUserService;
            _userManager = userManager;
        }

        public async Task<Response<IEnumerable<EventViewModel>>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUserService.UserId;

            // If ClubId is provided, check if user has authority in that club
            if (request.ClubId.HasValue)
            {
                var hasAuthority = await _clubRepository.HasAuthorityInClubAsync(request.ClubId.Value, userId);
                if (!hasAuthority)
                {
                    // If not club leader, they can only see events if they are an admin
                }
            }

            var query = _context.Events
                .Include(e => e.EventClubs).ThenInclude(ec => ec.Club)
                .AsQueryable();

            if (request.ClubId.HasValue)
            {
                query = query.Where(e => e.EventClubs.Any(ec => ec.ClubId == request.ClubId.Value));
            }

            // Visibility filter — sızıntıyı engelle, izinsiz etkinlikleri yok say
            query = await EventVisibilityFilter.ApplyAsync(query, _context, _userManager, userId, cancellationToken);

            var allEvents = await query.ToListAsync(cancellationToken);

            var viewModels = _mapper.Map<List<EventViewModel>>(allEvents);

            // Mevcut kullanıcının kayıt durumunu işaretle (tek sorgu, küçük IN listesi)
            if (!string.IsNullOrEmpty(userId) && viewModels.Count > 0)
            {
                var ids = viewModels.Select(v => v.Id).ToList();
                var registeredIds = await _context.EventAttendees
                    .Where(a => a.UserId == userId && ids.Contains(a.EventId))
                    .Select(a => a.EventId)
                    .ToListAsync(cancellationToken);
                var registeredSet = new HashSet<int>(registeredIds);
                foreach (var vm in viewModels)
                    vm.IsRegistered = registeredSet.Contains(vm.Id);
            }

            return new Response<IEnumerable<EventViewModel>>(viewModels);
        }
    }
}
