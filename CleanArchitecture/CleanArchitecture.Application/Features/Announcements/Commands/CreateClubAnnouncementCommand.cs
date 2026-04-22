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
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public CreateClubAnnouncementCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(CreateClubAnnouncementCommand request, CancellationToken cancellationToken)
        {
            var userId = request.CurrentUserId ?? _authenticatedUserService.UserId;

            if (!await _clubRepository.HasPrivilegeInClubAsync(request.ClubId, userId, "Send Announcements"))
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
