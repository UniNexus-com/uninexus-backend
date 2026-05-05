using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Commands.RegisterToEvent
{
    public class RegisterToEventCommand : IRequest<Response<string>>
    {
        public int EventId { get; set; }
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }
    }

    public class RegisterToEventCommandHandler : IRequestHandler<RegisterToEventCommand, Response<string>>
    {
        private const double MaxDistanceMeters = 200.0;
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public RegisterToEventCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUser)
        {
            _context = context;
            _authenticatedUser = authenticatedUser;
        }

        public async Task<Response<string>> Handle(RegisterToEventCommand request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUser.UserId;
            if (string.IsNullOrEmpty(userId))
                throw new ApiException("Kayıt olmak için giriş yapmalısınız.");

            var evt = await _context.Events.FindAsync(new object[] { request.EventId }, cancellationToken);
            if (evt == null)
                throw new EntityNotFoundException("Event", request.EventId);

            // ── Konum kontrolü ──
            // Etkinliğin koordinatları varsa, kullanıcı konumu zorunlu ve 100m mesafe limiti uygulanır.
            if (evt.Latitude.HasValue && evt.Longitude.HasValue)
            {
                if (!request.UserLatitude.HasValue || !request.UserLongitude.HasValue)
                    throw new ApiException("Bu etkinliğe kayıt olmak için konum izni vermeniz gerekmektedir.");

                var distance = HaversineDistance(
                    request.UserLatitude.Value, request.UserLongitude.Value,
                    evt.Latitude.Value, evt.Longitude.Value);

                if (distance > MaxDistanceMeters)
                    throw new ApiException("Etkinliğe çok uzaktasınız. Lütfen etkinlik konumuna yaklaşın.");
            }
            // Etkinliğin koordinatları yoksa konum kontrolü yapılmaz.

            var existing = await _context.EventAttendees
                .FirstOrDefaultAsync(x => x.EventId == request.EventId && x.UserId == userId, cancellationToken);

            if (existing != null)
                throw new ApiException("Bu etkinliğe zaten kayıt oldunuz.");

            var attendee = new EventAttendee
            {
                EventId = request.EventId,
                UserId = userId,
                Status = "Registered",
                PointsEarned = 0,
                CheckInTime = default(DateTime)
            };

            await _context.EventAttendees.AddAsync(attendee, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<string>(data: "Registered", message: "Etkinliğe başarıyla kayıt oldunuz.");
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