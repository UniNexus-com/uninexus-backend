using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Commands.ApproveEvent
{
    public class ApproveEventCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public bool IsApproved { get; set; }
        public string SksComment { get; set; }
    }

    public class ApproveEventCommandHandler : IRequestHandler<ApproveEventCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Event> _eventRepository;

        public ApproveEventCommandHandler(IGenericRepositoryAsync<Event> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<Response<int>> Handle(ApproveEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(request.Id);

            if (eventEntity == null)
            {
                throw new ApiException($"No event with ID {request.Id} was found.");
            }

            if (request.IsApproved)
            {
                eventEntity.Status = EventStatus.Approved; 
                eventEntity.IsActive = true;
            }
            else
            {
                eventEntity.Status = EventStatus.Rejected;
                eventEntity.IsActive = false;
            }

            // TODO: İleride buraya SignalR ile kulüp başkanına bildirim gitmesi için kod eklenecek.

            await _eventRepository.UpdateAsync(eventEntity);

            return new Response<int>(eventEntity.Id, request.IsApproved ? "The event has been successfully approved." : "Event rejected.");
        }
    }
}
