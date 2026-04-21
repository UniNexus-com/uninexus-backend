using AutoMapper;
using CleanArchitecture.Core.Exceptions;
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
        public int? ClubId { get; set; }
        public string Category { get; set; }
        public string Visibility { get; set; }
        public int? Capacity { get; set; }
        public string Requirements { get; set; }
        public bool RequireApproval { get; set; }
        public string Tags { get; set; }
    }

    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Entities.Event> _eventRepository;
        private readonly IMapper _mapper;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public CreateEventCommandHandler(IGenericRepositoryAsync<Entities.Event> eventRepository, IMapper mapper, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _eventRepository = eventRepository;
            _mapper = mapper;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            if (request.ClubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(request.ClubId.Value, _authenticatedUserService.UserId, "Manage Events"))
                    throw new ApiException("You do not have permission to create events in this club.");
            }

            var eventEntity = _mapper.Map<Entities.Event>(request);
            await _eventRepository.AddAsync(eventEntity);
            return new Response<int>(eventEntity.Id);
        }
    }
}
