using CleanArchitecture.Core.Wrappers;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Core.Interfaces;
using System.Threading;

namespace CleanArchitecture.Core.Features.Announcements.Commands
{
    public class CreateAnnouncementCommand : IRequest<Response<int>>
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public AnnouncementPriority Priority { get; set; }
    }

    public class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        public CreateAnnouncementCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Response<int>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
        {
            var announcement = new Announcement
            {
                Title = request.Title,
                Message = request.Message,
                Priority = request.Priority
            };
            await _context.Announcements.AddAsync(announcement, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return new Response<int>(announcement.Id);
        }
    }
}