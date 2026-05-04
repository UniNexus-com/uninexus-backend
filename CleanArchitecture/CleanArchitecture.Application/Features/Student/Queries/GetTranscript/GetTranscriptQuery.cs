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
            var data = await _context.Set<Entities.ApplicationUser>()
                .Where(x => x.Id == request.UserId)
                .Select(u => new TranscriptResponse
                {
                    StudentName = string.IsNullOrEmpty(u.FullName) ? u.UserName : u.FullName,
                    StudentNumber = u.StudentNumber,
                    TotalPoints = u.TotalScore,
                    Activities = _context.EventAttendees
                        .Where(a => a.UserId == u.Id)
                        .Include(a => a.Event)
                        .Select(a => new TranscriptEventItem
                        {
                            EventName = a.Event.Title,
                            Date = a.CheckInTime.ToString("dd/MM/yyyy"),
                            Points = a.PointsEarned
                        }).ToList()
                }).FirstOrDefaultAsync(cancellationToken);

            if (data == null) return new Response<TranscriptResponse>("Öğrenci bulunamadı.");

            return new Response<TranscriptResponse>(data);
        }
    }
}
