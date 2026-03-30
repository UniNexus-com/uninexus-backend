using AutoMapper;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Queries.GetAllEvents
{
    public class GetAllEventsQuery : IRequest<PagedResponse<EventViewModel>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetAllEventsQueryHandler : IRequestHandler<GetAllEventsQuery, PagedResponse<EventViewModel>>
    {
        private readonly IGenericRepositoryAsync<Entities.Event> _eventRepository;
        private readonly IMapper _mapper;

        public GetAllEventsQueryHandler(IGenericRepositoryAsync<Entities.Event> eventRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<EventViewModel>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
        {
            // Set default pagination if not provided
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var eventItems = await _eventRepository.GetPagedReponseAsync(pageNumber, pageSize);
            var eventViewModel = _mapper.Map<IEnumerable<EventViewModel>>(eventItems);
            
            return new PagedResponse<EventViewModel>(new List<EventViewModel>(eventViewModel), pageNumber, pageSize);
        }
    }
}
