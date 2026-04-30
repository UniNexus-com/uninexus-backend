using CleanArchitecture.Core.DTOs.Transcript;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Users.Queries.GetRecentActivities
{
    public class GetRecentActivitiesQuery : IRequest<Response<IEnumerable<TranscriptEventItem>>>
    {
        public string UserId { get; set; }
    }

    public class GetRecentActivitiesQueryHandler : IRequestHandler<GetRecentActivitiesQuery, Response<IEnumerable<TranscriptEventItem>>>
    {
        private readonly IApplicationDbContext _context;

        public GetRecentActivitiesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<TranscriptEventItem>>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
        {
            var activities = await _context.EventAttendees
                .Where(a => a.UserId == request.UserId && a.Status == "Attended")
                .Include(a => a.Event)
                .OrderByDescending(a => a.CheckInTime)
                .Take(5)
                .Select(a => new TranscriptEventItem
                {
                    EventName = a.Event.Title,
                    Date = a.CheckInTime.ToString("dd/MM/yyyy HH:mm"),
                    Points = a.PointsEarned
                })
                .ToListAsync(cancellationToken);

            return new Response<IEnumerable<TranscriptEventItem>>(activities);
        }
    }
}
