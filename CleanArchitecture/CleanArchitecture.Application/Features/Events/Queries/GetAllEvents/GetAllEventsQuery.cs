using AutoMapper;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
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
        private readonly IMapper _mapper;

        public GetAllEventsQueryHandler(
            IApplicationDbContext context, 
            IClubRepositoryAsync clubRepository,
            IMapper mapper, 
            IAuthenticatedUserService authenticatedUserService)
        {
            _context = context;
            _clubRepository = clubRepository;
            _mapper = mapper;
            _authenticatedUserService = authenticatedUserService;
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

            var query = _context.Events.Include(e => e.Club).AsQueryable();

            if (request.ClubId.HasValue)
            {
                query = query.Where(e => e.ClubId == request.ClubId.Value);
            }

            var allEvents = await query.ToListAsync(cancellationToken);

            var viewModels = _mapper.Map<IEnumerable<EventViewModel>>(allEvents);
            return new Response<IEnumerable<EventViewModel>>(viewModels);
        }
    }
}
