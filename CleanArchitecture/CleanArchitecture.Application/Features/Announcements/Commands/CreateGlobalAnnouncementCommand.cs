using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Announcements.Commands
{
    public class CreateGlobalAnnouncementCommand : IRequest<Response<int>>
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public AnnouncementPriority Priority { get; set; }
    }

    public class CreateGlobalAnnouncementCommandHandler : IRequestHandler<CreateGlobalAnnouncementCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        public CreateGlobalAnnouncementCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<int>> Handle(CreateGlobalAnnouncementCommand request, CancellationToken cancellationToken)
        {
            var announcement = new Announcement
            {
                Title = request.Title,
                Message = request.Message,
                Priority = request.Priority,
                ClubId = null
            };

            await _context.Announcements.AddAsync(announcement, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<int>(announcement.Id);
        }
    }
}
