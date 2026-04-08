using CleanArchitecture.Core.Entities;
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

        public UpdateBudgetRequestStatusCommandHandler(IGenericRepositoryAsync<BudgetRequest> repo)
        {
            _repo = repo;
        }

        public async Task<Response<int>> Handle(UpdateBudgetRequestStatusCommand request, CancellationToken cancellationToken)
        {
            var all = await _repo.GetAllAsync();
            var entity = all.FirstOrDefault(r => r.Id == request.Id);
            if (entity == null) return new Response<int>("Budget request not found.");
            entity.Status = request.Status;
            await _repo.UpdateAsync(entity);
            return new Response<int>(entity.Id);
        }
    }
}
