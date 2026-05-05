using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Core.Features.Events.Commands.CheckInEvent
{
    public class CheckInToEventCommand : IRequest<Response<string>>
    {
        public int EventId { get; set; }
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }
    }

    public class CheckInToEventCommandHandler : IRequestHandler<CheckInToEventCommand, Response<string>>
    {
        private const double MaxDistanceMeters = 200.0;
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
                throw new ApiException("You must be logged in to check in.");

            var evt = await _context.Events.FindAsync(new object[] { request.EventId }, cancellationToken);
            if (evt == null)
                throw new EntityNotFoundException("Event", request.EventId);

            if (evt.Latitude.HasValue && evt.Longitude.HasValue
                && request.UserLatitude.HasValue && request.UserLongitude.HasValue)
            {
                var distance = HaversineDistance(
                    request.UserLatitude.Value, request.UserLongitude.Value,
                    evt.Latitude.Value, evt.Longitude.Value);
                if (distance > MaxDistanceMeters)
                    throw new ApiException("Etkinliğe çok uzaktasınız. Lütfen etkinlik konumuna yaklaşın.");
            }

            var existingCheckIn = await _context.EventAttendees
                .FirstOrDefaultAsync(x => x.EventId == request.EventId && x.UserId == userId, cancellationToken);

            if (existingCheckIn != null)
                throw new ApiException("You are already registered for this event.");

            var user = await _context.Set<ApplicationUser>().FindAsync(new object[] { userId }, cancellationToken);
            if (user == null)
                throw new ApiException("User profile not found.");

            int pointsToAward = 250;
            user.ScoreWalletBalance += pointsToAward;
            user.TotalScore += pointsToAward;

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
                message: $"Successfully checked in to '{evt.Title}'. You've earned {pointsToAward} points!"
            );
        }

        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}