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

        public DeleteEventCommandHandler(IGenericRepositoryAsync<Entities.Event> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<Response<int>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var eventItem = await _eventRepository.GetByIdAsync(request.Id);
            
            if (eventItem == null)
            {
                throw new ApiException($"Event Not Found.");
            }

            await _eventRepository.DeleteAsync(eventItem);
            return new Response<int>(eventItem.Id);
        }
    }
}
