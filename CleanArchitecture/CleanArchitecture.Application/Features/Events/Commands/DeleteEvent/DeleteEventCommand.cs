using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Commands.DeleteEvent
{
    public class DeleteEventCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }

    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Entities.Event> _eventRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public DeleteEventCommandHandler(IGenericRepositoryAsync<Entities.Event> eventRepository, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _eventRepository = eventRepository;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var eventItem = await _eventRepository.GetByIdAsync(request.Id);
            
            if (eventItem == null)
            {
                throw new ApiException($"Event Not Found.");
            }

            if (eventItem.ClubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(eventItem.ClubId.Value, _authenticatedUserService.UserId, "Manage Events"))
                    throw new ApiException("You do not have permission to delete events in this club.");
            }

            await _eventRepository.DeleteAsync(eventItem);
            return new Response<int>(eventItem.Id);
        }
    }
}
