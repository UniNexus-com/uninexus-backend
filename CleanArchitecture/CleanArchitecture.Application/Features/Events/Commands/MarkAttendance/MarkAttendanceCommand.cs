using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Commands.MarkAttendance
{
    public class MarkAttendanceCommand : IRequest<Response<string>>
    {
        public int EventId { get; set; }
        public string UserId { get; set; }
        public bool Attended { get; set; }
    }

    public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, Response<string>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public MarkAttendanceCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUser)
        {
            _context = context;
            _authenticatedUser = authenticatedUser;
        }

        public async Task<Response<string>> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
        {
            // Find the attendee record — EF Core tracks by composite key (UserId, EventId)
            var attendee = await _context.EventAttendees
                .FirstOrDefaultAsync(x => x.EventId == request.EventId && x.UserId == request.UserId, cancellationToken);

            if (attendee == null)
                throw new ApiException("Bu kullanıcı etkinliğe kayıtlı değil.");

            attendee.Status = request.Attended ? "Attended" : "Registered";
            attendee.CheckInTime = request.Attended ? DateTime.UtcNow : default(DateTime);

            _context.EventAttendees.Update(attendee);
            await _context.SaveChangesAsync(cancellationToken);

            var message = request.Attended
                ? "Katılım başarıyla işaretlendi."
                : "Katılım işareti kaldırıldı.";

            return new Response<string>(data: attendee.Status, message: message);
        }
    }
}
