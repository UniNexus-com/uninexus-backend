using AutoMapper;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommand : IRequest<Response<int>>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Entities.Event> _eventRepository;
        private readonly IMapper _mapper;

        public CreateEventCommandHandler(IGenericRepositoryAsync<Entities.Event> eventRepository, AutoMapper.IMapper mapper)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public async Task<Response<int>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = _mapper.Map<Entities.Event>(request);
            await _eventRepository.AddAsync(eventEntity);
            return new Response<int>(eventEntity.Id);
        }
    }
}
