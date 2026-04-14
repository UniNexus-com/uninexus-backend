using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Core.Features.Events.Commands.CheckInEvent
{
    public class CheckInToEventCommand : IRequest<Response<string>>
    {
        public int EventId { get; set; }
    }

    public class CheckInToEventCommandHandler : IRequestHandler<CheckInToEventCommand, Response<string>>
    {

        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public CheckInToEventCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUser)
        {
            _context = context;
            _authenticatedUser = authenticatedUser;
        }
        public async Task<Response<string>> Handle(CheckInToEventCommand request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUser.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                throw new ApiException("You must be logged in to check in.");
            }

            var evt = await _context.Events.FindAsync(new object[] { request.EventId }, cancellationToken);
            if (evt == null)
            {
                throw new EntityNotFoundException("Event", request.EventId);
            }

            var existingCheckIn = await _context.EventAttendees
                .FirstOrDefaultAsync(x => x.EventId == request.EventId && x.UserId == userId, cancellationToken);

            if (existingCheckIn != null)
            {
                throw new ApiException("You are already registered for this event.");
            }

            var user = await _context.Set<ApplicationUser>().FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
                throw new ApiException("User profile not found.");

            int pointsToAward = 10;
            user.ScoreWalletBalance += pointsToAward;

            _context.Set<ApplicationUser>().Update(user);

            var attendee = new EventAttendee
            {
                EventId = request.EventId,
                UserId = userId,
                CheckInTime = DateTime.UtcNow,
                PointsEarned = pointsToAward,
                Status = "Attended"
            };

            await _context.EventAttendees.AddAsync(attendee, cancellationToken);


            await _context.SaveChangesAsync(cancellationToken);

            return new Response<string>(
                data: user.ScoreWalletBalance.ToString(),
                message: $"Successfully checked in to event. You've earned {pointsToAward} points!"
                );
        }
    }
}
