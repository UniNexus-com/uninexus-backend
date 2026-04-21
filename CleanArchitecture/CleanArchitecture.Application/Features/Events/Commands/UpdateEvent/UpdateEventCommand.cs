using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Commands.UpdateEvent
{
    public class UpdateEventCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
        public string Visibility { get; set; }
        public int? Capacity { get; set; }
        public string Requirements { get; set; }
        public bool RequireApproval { get; set; }
        public string Tags { get; set; }
    }

    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Entities.Event> _eventRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public UpdateEventCommandHandler(IGenericRepositoryAsync<Entities.Event> eventRepository, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _eventRepository = eventRepository;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var eventItem = await _eventRepository.GetByIdAsync(request.Id);

            if (eventItem == null)
            {
                throw new ApiException($"Event Not Found.");
            }

            if (eventItem.ClubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(eventItem.ClubId.Value, _authenticatedUserService.UserId, "Manage Events"))
                    throw new ApiException("You do not have permission to update events in this club.");
            }

            eventItem.Title = request.Title;
            eventItem.Description = request.Description;
            eventItem.StartDate = request.StartDate;
            eventItem.EndDate = request.EndDate;
            eventItem.Location = request.Location;
            eventItem.IsActive = request.IsActive;
            eventItem.Category = request.Category;
            eventItem.Visibility = request.Visibility;
            eventItem.Capacity = request.Capacity;
            eventItem.Requirements = request.Requirements;
            eventItem.RequireApproval = request.RequireApproval;
            eventItem.Tags = request.Tags;

            await _eventRepository.UpdateAsync(eventItem);
            return new Response<int>(eventItem.Id);
        }
    }
}
