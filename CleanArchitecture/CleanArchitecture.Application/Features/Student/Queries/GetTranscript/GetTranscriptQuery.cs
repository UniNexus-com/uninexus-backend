using CleanArchitecture.Core.DTOs.Transcript;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Student.Queries.GetTranscript
{
    public class GetTranscriptQuery : IRequest<Response<TranscriptResponse>>
    {
        public string UserId { get; set; }
    }

    public class GetTranscriptQueryHandler : IRequestHandler<GetTranscriptQuery, Response<TranscriptResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetTranscriptQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<TranscriptResponse>> Handle(GetTranscriptQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Set<Entities.ApplicationUser>()
                .Where(x => x.Id == request.UserId)
                .Select(u => new { u.Id, u.FullName, u.UserName, u.StudentNumber, u.TotalScore })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null) return new Response<TranscriptResponse>("Öğrenci bulunamadı.");

            var eventActivities = await _context.EventAttendees
                .Where(a => a.UserId == user.Id)
                .Include(a => a.Event)
                .Select(a => new TranscriptEventItem
                {
                    EventName = a.Event.Title,
                    Date = a.CheckInTime.ToString("dd/MM/yyyy"),
                    Points = a.PointsEarned,
                    Category = "Event"
                }).ToListAsync(cancellationToken);

            var foundedClubs = await _context.ClubCreationRequests
                .Where(r => r.RequesterUserId == user.Id && r.Status == "APPROVED")
                .Select(r => new TranscriptEventItem
                {
                    EventName = $"Club Founded: {r.Name}",
                    Date = r.Created.ToString("dd/MM/yyyy"),
                    Points = 1000,
                    Category = "Club"
                }).ToListAsync(cancellationToken);

            var clubMemberships = await _context.UserClubs
                .Where(uc => uc.UserId == user.Id && uc.IsActive)
                .Include(uc => uc.Club)
                .Include(uc => uc.Role)
                .Select(uc => new TranscriptEventItem
                {
                    EventName = $"{uc.Club.Name} — {uc.Role.Name}",
                    Date = uc.JoinDate.ToString("dd/MM/yyyy"),
                    Points = 0,
                    Category = "Membership"
                }).ToListAsync(cancellationToken);

            var allActivities = eventActivities
                .Concat(foundedClubs)
                .Concat(clubMemberships)
                .OrderBy(a => a.Date)
                .ToList();

            var listedTotal = allActivities.Sum(a => a.Points);
            var diff = user.TotalScore - listedTotal;
            if (diff > 0)
            {
                allActivities.Add(new TranscriptEventItem
                {
                    EventName = "Kulüp İçi Terfiler",
                    Date = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                    Points = diff,
                    Category = "Promotion"
                });
            }

            var data = new TranscriptResponse
            {
                StudentName = string.IsNullOrEmpty(user.FullName) ? user.UserName : user.FullName,
                StudentNumber = user.StudentNumber,
                TotalPoints = user.TotalScore,
                Activities = allActivities
            };

            return new Response<TranscriptResponse>(data);
        }
    }
}
