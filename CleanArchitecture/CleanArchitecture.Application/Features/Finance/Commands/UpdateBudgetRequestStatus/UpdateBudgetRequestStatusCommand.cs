using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateBudgetRequestStatusCommandHandler(
            IGenericRepositoryAsync<BudgetRequest> repo,
            IAuthenticatedUserService authenticatedUserService,
            IClubRepositoryAsync clubRepository,
            UserManager<ApplicationUser> userManager)
        {
            _repo = repo;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
            _userManager = userManager;
        }

        public async Task<Response<int>> Handle(UpdateBudgetRequestStatusCommand request, CancellationToken cancellationToken)
        {
            var all = await _repo.GetAllAsync();
            var entity = all.FirstOrDefault(r => r.Id == request.Id);
            if (entity == null) return new Response<int>("Budget request not found.");

            // Yetki kontrolü: BudgetRequest'in ait olduğu kulüpte 'Manage Finances' yetkisi gerekli
            // VEYA kullanıcı SKS_ADMIN identity rolüne sahip olmalı
            if (entity.ClubId.HasValue)
            {
                var userId = _authenticatedUserService.UserId;
                
                // Admin kontrolü
                var user = await _userManager.FindByIdAsync(userId);
                var roles = user != null ? await _userManager.GetRolesAsync(user) : new System.Collections.Generic.List<string>();
                var isSksAdmin = roles.Contains("SKS_ADMIN");

                if (!isSksAdmin)
                {
                    var hasPrivilege = await _clubRepository.HasPrivilegeInClubAsync(entity.ClubId.Value, userId, "Manage Finances");
                    if (!hasPrivilege)
                        throw new ApiException("You do not have permission to update budget request status in this club.");
                }
            }

            entity.Status = request.Status;
            await _repo.UpdateAsync(entity);
            return new Response<int>(entity.Id);
        }
    }
}
