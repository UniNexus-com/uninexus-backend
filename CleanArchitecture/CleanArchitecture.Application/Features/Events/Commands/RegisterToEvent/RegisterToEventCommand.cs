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
    }

    public class RegisterToEventCommandHandler : IRequestHandler<RegisterToEventCommand, Response<string>>
    {
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
    }
}
