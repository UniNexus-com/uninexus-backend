using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Finance.Commands.UpdateBudgetRequestStatus
{
    public class UpdateBudgetRequestStatusCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }

    public class UpdateBudgetRequestStatusCommandHandler : IRequestHandler<UpdateBudgetRequestStatusCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<BudgetRequest> _repo;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public UpdateBudgetRequestStatusCommandHandler(
            IGenericRepositoryAsync<BudgetRequest> repo,
            IAuthenticatedUserService authenticatedUserService,
            IClubRepositoryAsync clubRepository)
        {
            _repo = repo;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(UpdateBudgetRequestStatusCommand request, CancellationToken cancellationToken)
        {
            var all = await _repo.GetAllAsync();
            var entity = all.FirstOrDefault(r => r.Id == request.Id);
            if (entity == null) return new Response<int>("Budget request not found.");

            // Yetki kontrolü: BudgetRequest'in ait olduğu kulüpte 'Manage Budget' yetkisi gerekli
            if (entity.ClubId.HasValue)
            {
                var userId = _authenticatedUserService.UserId;
                var hasPrivilege = await _clubRepository.HasPrivilegeInClubAsync(entity.ClubId.Value, userId, "Manage Budget");
                if (!hasPrivilege)
                    throw new ApiException("You do not have permission to update budget request status in this club.");
            }

            entity.Status = request.Status;
            await _repo.UpdateAsync(entity);
            return new Response<int>(entity.Id);
        }
    }
}
