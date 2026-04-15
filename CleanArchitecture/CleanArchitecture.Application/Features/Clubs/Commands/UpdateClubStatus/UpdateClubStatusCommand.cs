using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.UpdateClubStatus
{
    public class UpdateClubStatusCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public string Status { get; set; } // "ACTIVE" | "PENDING" | "CLOSED"
    }

    public class UpdateClubStatusCommandHandler : IRequestHandler<UpdateClubStatusCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Club> _clubRepo;

        public UpdateClubStatusCommandHandler(IGenericRepositoryAsync<Club> clubRepo)
        {
            _clubRepo = clubRepo;
        }

        public async Task<Response<int>> Handle(UpdateClubStatusCommand command, CancellationToken cancellationToken)
        {
            var validStatuses = new[] { "ACTIVE", "PENDING", "CLOSED" };
            var normalizedStatus = command.Status?.ToUpper();

            if (!validStatuses.Contains(normalizedStatus))
                return new Response<int>("Invalid status. Allowed values: ACTIVE, PENDING, CLOSED.");

            var club = await _clubRepo.GetByIdAsync(command.Id);
            if (club == null) return new Response<int>("Club not found.");

            club.Status = normalizedStatus;
            club.IsActive = normalizedStatus == "ACTIVE";

            await _clubRepo.UpdateAsync(club);

            return new Response<int>(club.Id);
        }
    }
}
