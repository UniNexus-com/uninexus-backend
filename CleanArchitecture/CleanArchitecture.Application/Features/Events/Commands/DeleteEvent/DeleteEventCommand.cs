using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Features.Events.Common;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Commands.DeleteEvent
{
    public class DeleteEventCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }

    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public DeleteEventCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var eventItem = await _context.Events
                .AsTracking()
                .Include(e => e.EventClubs)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (eventItem == null)
                throw new ApiException($"Event Not Found.");

            await EventManagementPermissions.EnsureCanManageEventAsync(eventItem, _authenticatedUserService.UserId, _clubRepository);

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync(cancellationToken);
            return new Response<int>(eventItem.Id);
        }
    }
}
