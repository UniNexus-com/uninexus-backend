using AutoMapper;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
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
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetEventByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<EventViewModel>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            var eventItem = await _context.Events
                .Include(e => e.Club)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (eventItem == null) throw new CleanArchitecture.Core.Exceptions.ApiException($"Event Not Found.");
            
            var eventViewModel = _mapper.Map<EventViewModel>(eventItem);
            return new Response<EventViewModel>(eventViewModel);
        }
    }
}
