using CleanArchitecture.Core.DTOs.Announcement;
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

namespace CleanArchitecture.Core.Features.Announcements.Queries
{
    public class GetClubAnnouncementsQuery : IRequest<Response<List<AnnouncementResponse>>>
    {
        public int ClubId { get; set; }
    }

    public class GetClubAnnouncementsQueryHandler : IRequestHandler<GetClubAnnouncementsQuery, Response<List<AnnouncementResponse>>>
    {
        private readonly IApplicationDbContext _context;
        public GetClubAnnouncementsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Response<List<AnnouncementResponse>>> Handle(GetClubAnnouncementsQuery request, CancellationToken cancellationToken)
        {
            var data = await _context.Announcements
                .Where(x => x.ClubId == request.ClubId)
                .OrderByDescending(x => x.Created)
                .Select(x => new AnnouncementResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Message = x.Message,
                    Priority = x.Priority,
                    CreatedAt = x.Created
                }).ToListAsync(cancellationToken);

            return new Response<List<AnnouncementResponse>>(data);
        }
    }
}
