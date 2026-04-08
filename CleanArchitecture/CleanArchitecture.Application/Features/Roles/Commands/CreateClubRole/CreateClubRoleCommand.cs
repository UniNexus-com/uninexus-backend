using AutoMapper;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Commands.CreateClubRole
{
    public class CreateClubRoleCommand : IRequest<Response<int>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public int? ClubId { get; set; }
    }

    public class CreateClubRoleCommandHandler : IRequestHandler<CreateClubRoleCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<ClubRole> _repo;
        private readonly IMapper _mapper;

        public CreateClubRoleCommandHandler(IGenericRepositoryAsync<ClubRole> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<Response<int>> Handle(CreateClubRoleCommand request, CancellationToken cancellationToken)
        {
            var role = _mapper.Map<ClubRole>(request);
            role.IsSystemRole = false;
            await _repo.AddAsync(role);
            return new Response<int>(role.Id);
        }
    }
}
