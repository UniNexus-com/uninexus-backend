using AutoMapper;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
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
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly IClubRepositoryAsync _clubRepository;

        public CreateBudgetRequestCommandHandler(IGenericRepositoryAsync<BudgetRequest> repo, IMapper mapper, IAuthenticatedUserService authenticatedUserService, IClubRepositoryAsync clubRepository)
        {
            _repo = repo;
            _mapper = mapper;
            _authenticatedUserService = authenticatedUserService;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(CreateBudgetRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.ClubId.HasValue)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(request.ClubId.Value, _authenticatedUserService.UserId, "Manage Budget"))
                    throw new ApiException("You do not have permission to create budget requests for this club.");
            }

            var entity = _mapper.Map<BudgetRequest>(request);
            entity.Status = "PENDING";
            await _repo.AddAsync(entity);
            return new Response<int>(entity.Id);
        }
    }
}
