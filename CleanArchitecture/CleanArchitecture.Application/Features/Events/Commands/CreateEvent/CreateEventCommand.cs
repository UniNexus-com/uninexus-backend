using AutoMapper;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
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
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsActive { get; set; }
        /// <summary>Eski tek-kulüp API uyumluluğu; <see cref="HostClubIds"/> ile birleştirilir.</summary>
        public int? ClubId { get; set; }
        /// <summary>Ortak/çok kulüplü düzen; boş ise üniversite etkinliği (kulüp zorunluluğu yok).</summary>
        public ICollection<int> HostClubIds { get; set; }
        public string Category { get; set; }
        public string Visibility { get; set; }
        public int? Capacity { get; set; }
        public string Requirements { get; set; }
        public bool RequireApproval { get; set; }
        public string Tags { get; set; }
        public string CoverImageUrl { get; set; }
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
            var hostClubIds = MergeHostClubIds(request);
            var userId = _authenticatedUserService.UserId;

            foreach (var clubId in hostClubIds)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(clubId, userId, "Manage Events"))
                    throw new ApiException("You do not have permission to create events for one of these clubs.");
            }

            var eventEntity = _mapper.Map<Entities.Event>(request);
            for (var i = 0; i < hostClubIds.Count; i++)
                eventEntity.EventClubs.Add(new EventClub { ClubId = hostClubIds[i], SortOrder = i });

            await _eventRepository.AddAsync(eventEntity);
            return new Response<int>(eventEntity.Id);
        }

        private static List<int> MergeHostClubIds(CreateEventCommand request)
        {
            var list = new List<int>();
            if (request.HostClubIds != null)
            {
                foreach (var id in request.HostClubIds)
                    if (!list.Contains(id))
                        list.Add(id);
            }
            if (request.ClubId.HasValue && !list.Contains(request.ClubId.Value))
                list.Add(request.ClubId.Value);
            return list;
        }
    }
}
