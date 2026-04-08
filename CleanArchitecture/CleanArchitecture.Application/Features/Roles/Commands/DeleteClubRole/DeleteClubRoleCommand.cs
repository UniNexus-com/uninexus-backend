using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Commands.DeleteClubRole
{
    public class DeleteClubRoleCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }

    public class DeleteClubRoleCommandHandler : IRequestHandler<DeleteClubRoleCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<ClubRole> _repo;

        public DeleteClubRoleCommandHandler(IGenericRepositoryAsync<ClubRole> repo)
        {
            _repo = repo;
        }

        public async Task<Response<int>> Handle(DeleteClubRoleCommand request, CancellationToken cancellationToken)
        {
            var all = await _repo.GetAllAsync();
            var role = all.FirstOrDefault(r => r.Id == request.Id);
            if (role == null) return new Response<int>("Role not found.");
            if (role.IsSystemRole) return new Response<int>("Cannot delete a system role.");
            await _repo.DeleteAsync(role);
            return new Response<int>(role.Id);
        }
    }
}
