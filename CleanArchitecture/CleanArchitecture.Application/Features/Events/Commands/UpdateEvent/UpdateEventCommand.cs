using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Features.Events.Common;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Commands.UpdateEvent
{
    public class UpdateEventCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }
        public string Visibility { get; set; }
        public int? Capacity { get; set; }
        public string Requirements { get; set; }
        public bool RequireApproval { get; set; }
        public string Tags { get; set; }
    }

    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public UpdateEventCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _context = context;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var eventItem = await _context.Events
                .AsTracking()
                .Include(e => e.EventClubs)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (eventItem == null)
                throw new ApiException($"Event Not Found.");

            await EventManagementPermissions.EnsureCanManageEventAsync(eventItem, _authenticatedUserService.UserId, _clubRepository);

            eventItem.Title = request.Title;
            eventItem.Description = request.Description;
            eventItem.StartDate = request.StartDate;
            eventItem.EndDate = request.EndDate;
            eventItem.Location = request.Location;
            eventItem.IsActive = request.IsActive;
            eventItem.Category = request.Category;
            eventItem.Visibility = request.Visibility;
            eventItem.Capacity = request.Capacity;
            eventItem.Requirements = request.Requirements;
            eventItem.RequireApproval = request.RequireApproval;
            eventItem.Tags = request.Tags;

            await _context.SaveChangesAsync(cancellationToken);
            return new Response<int>(eventItem.Id);
        }
    }
}
