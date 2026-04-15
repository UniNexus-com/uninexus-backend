using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
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

namespace CleanArchitecture.Core.Features.Announcements.Commands
{
    public class CreateClubAnnouncementCommand : IRequest<Response<int>>
    {
        public int ClubId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public AnnouncementPriority Priority { get; set; }
        public string CurrentUserId { get; set; }
    }

    public class CreateClubAnnouncementCommandHandler : IRequestHandler<CreateClubAnnouncementCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        public CreateClubAnnouncementCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Response<int>> Handle(CreateClubAnnouncementCommand request, CancellationToken cancellationToken)
        {
            var isLeader = await _context.UserClubs
                .AnyAsync(uc =>
                    uc.ClubId == request.ClubId &&
                    uc.UserId == request.CurrentUserId,
                    cancellationToken);

            if (!isLeader)
                return new Response<int>("You do not have the authority to make announcements to this club!");

            var announcement = new Announcement
            {
                Title = request.Title,
                Message = request.Message,
                Priority = request.Priority,
                ClubId = request.ClubId
            };

            await _context.Announcements.AddAsync(announcement, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<int>(announcement.Id);
        }
    }
}
