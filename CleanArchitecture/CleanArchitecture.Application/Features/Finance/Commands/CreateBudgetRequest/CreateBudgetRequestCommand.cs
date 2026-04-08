using AutoMapper;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Finance.Commands.CreateBudgetRequest
{
    public class CreateBudgetRequestCommand : IRequest<Response<int>>
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int? ClubId { get; set; }
    }

    public class CreateBudgetRequestCommandHandler : IRequestHandler<CreateBudgetRequestCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<BudgetRequest> _repo;
        private readonly IMapper _mapper;

        public CreateBudgetRequestCommandHandler(IGenericRepositoryAsync<BudgetRequest> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Response<int>> Handle(CreateBudgetRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<BudgetRequest>(request);
            entity.Status = "PENDING";
            await _repo.AddAsync(entity);
            return new Response<int>(entity.Id);
        }
    }
}
