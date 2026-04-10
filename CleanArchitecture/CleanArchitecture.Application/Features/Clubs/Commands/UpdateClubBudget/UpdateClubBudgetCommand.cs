using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.UpdateClubBudget
{
    public class UpdateClubBudgetCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public decimal TotalBudget { get; set; }
    }

    public class UpdateClubBudgetCommandHandler : IRequestHandler<UpdateClubBudgetCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Club> _clubRepo;

        public UpdateClubBudgetCommandHandler(IGenericRepositoryAsync<Club> clubRepo)
        {
            _clubRepo = clubRepo;
        }

        public async Task<Response<int>> Handle(UpdateClubBudgetCommand command, CancellationToken cancellationToken)
        {
            var club = await _clubRepo.GetByIdAsync(command.Id);
            if (club == null) return new Response<int>("Club not found.");

            club.TotalBudget = command.TotalBudget;
            await _clubRepo.UpdateAsync(club);

            return new Response<int>(club.Id);
        }
    }
}
