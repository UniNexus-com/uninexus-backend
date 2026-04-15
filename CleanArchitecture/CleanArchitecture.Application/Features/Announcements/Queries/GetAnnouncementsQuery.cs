using CleanArchitecture.Core.DTOs.Announcement;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using Microsoft.EntityFrameworkCore;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Announcements.Queries
{
    public class GetAnnouncementsQuery : IRequest<Response<List<AnnouncementResponse>>>
    {
    }

    public class GetAnnouncementsQueryHandler : IRequestHandler<GetAnnouncementsQuery, Response<List<AnnouncementResponse>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAnnouncementsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<List<AnnouncementResponse>>> Handle(GetAnnouncementsQuery request, CancellationToken cancellationToken)
        {
            var data = await _context.Announcements
                .OrderByDescending(a => a.Created)
                .Select(x => new AnnouncementResponse
                {
                    Id = x.Id,
                    CreatedAt = x.Created,
                    Title = x.Title,
                    Message = x.Message,
                    Priority = x.Priority
                })
                .ToListAsync(cancellationToken);

            return new Response<List<AnnouncementResponse>>(data);
        }
    }
}
