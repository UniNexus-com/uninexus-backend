using CleanArchitecture.Core.Entities;
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

        public DeleteBudgetRequestCommandHandler(IGenericRepositoryAsync<BudgetRequest> repo)
        {
            _repo = repo;
        }

        public async Task<Response<int>> Handle(DeleteBudgetRequestCommand request, CancellationToken cancellationToken)
        {
            var all = await _repo.GetAllAsync();
            var entity = all.FirstOrDefault(r => r.Id == request.Id);
            if (entity == null) return new Response<int>("Budget request not found.");
            await _repo.DeleteAsync(entity);
            return new Response<int>(entity.Id);
        }
    }
}
