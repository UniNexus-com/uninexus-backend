using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Queries.GetEventAttendees
{
    public class GetEventAttendeesQuery : IRequest<Response<IEnumerable<EventAttendeeDto>>>
    {
        public int EventId { get; set; }
    }

    public class GetEventAttendeesQueryHandler : IRequestHandler<GetEventAttendeesQuery, Response<IEnumerable<EventAttendeeDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetEventAttendeesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<EventAttendeeDto>>> Handle(GetEventAttendeesQuery request, CancellationToken cancellationToken)
        {
            var attendees = await (
                from ea in _context.EventAttendees
                join u in _context.Set<ApplicationUser>() on ea.UserId equals u.Id
                where ea.EventId == request.EventId
                orderby ea.Created descending
                select new EventAttendeeDto
                {
                    UserId = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Status = ea.Status,
                    RegisteredAt = ea.Created,
                    CheckInTime = ea.CheckInTime == default(DateTime) ? (DateTime?)null : ea.CheckInTime
                }
            ).ToListAsync(cancellationToken);

            return new Response<IEnumerable<EventAttendeeDto>>(attendees);
        }
    }
}
