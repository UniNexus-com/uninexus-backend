using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Finance.Commands.DeleteBudgetRequest
{
    public class DeleteBudgetRequestCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }

    public class DeleteBudgetRequestCommandHandler : IRequestHandler<DeleteBudgetRequestCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<BudgetRequest> _repo;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public DeleteBudgetRequestCommandHandler(IGenericRepositoryAsync<BudgetRequest> repo, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _repo = repo;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(DeleteBudgetRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repo.GetByIdAsync(request.Id);
            if (entity == null) return new Response<int>("Budget request not found.");

            if (entity.ClubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(entity.ClubId.Value, _authenticatedUserService.UserId, "Manage Finances"))
                    throw new ApiException("You do not have permission to delete budget requests for this club.");
            }

            await _repo.DeleteAsync(entity);
            return new Response<int>(entity.Id);
        }
    }
}
