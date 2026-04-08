using AutoMapper;
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
        private readonly IGenericRepositoryAsync<Entities.Event> _eventRepository;
        private readonly IMapper _mapper;

        public GetAllEventsQueryHandler(IGenericRepositoryAsync<Entities.Event> eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<EventViewModel>>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
        {
            var allEvents = await _eventRepository.GetAllAsync();
            var filtered = request.ClubId.HasValue
                ? allEvents.Where(e => e.ClubId == request.ClubId)
                : allEvents;
            var viewModels = _mapper.Map<IEnumerable<EventViewModel>>(filtered);
            return new Response<IEnumerable<EventViewModel>>(viewModels);
        }
    }
}
