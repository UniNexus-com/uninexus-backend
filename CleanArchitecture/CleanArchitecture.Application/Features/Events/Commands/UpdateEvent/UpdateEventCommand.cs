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
    }

    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Entities.Event> _eventRepository;

        public UpdateEventCommandHandler(IGenericRepositoryAsync<Entities.Event> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<Response<int>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var eventItem = await _eventRepository.GetByIdAsync(request.Id);

            if (eventItem == null)
            {
                throw new ApiException($"Event Not Found.");
            }

            eventItem.Title = request.Title;
            eventItem.Description = request.Description;
            eventItem.StartDate = request.StartDate;
            eventItem.EndDate = request.EndDate;
            eventItem.Location = request.Location;
            eventItem.IsActive = request.IsActive;

            await _eventRepository.UpdateAsync(eventItem);
            return new Response<int>(eventItem.Id);
        }
    }
}
