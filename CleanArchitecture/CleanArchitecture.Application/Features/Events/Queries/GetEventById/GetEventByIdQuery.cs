using AutoMapper;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
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
        private readonly IGenericRepositoryAsync<Entities.Event> _eventRepository;
        private readonly IMapper _mapper;

        public GetEventByIdQueryHandler(IGenericRepositoryAsync<Entities.Event> eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public async Task<Response<EventViewModel>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            var eventItem = await _eventRepository.GetByIdAsync(request.Id);
            if (eventItem == null) throw new CleanArchitecture.Core.Exceptions.ApiException($"Event Not Found.");
            
            var eventViewModel = _mapper.Map<EventViewModel>(eventItem);
            return new Response<EventViewModel>(eventViewModel);
        }
    }
}
