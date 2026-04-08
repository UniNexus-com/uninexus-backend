using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IApplicationDbContext _dbContext;

        public DeleteClubRoleCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<int>> Handle(DeleteClubRoleCommand request, CancellationToken cancellationToken)
        {
            var role = await _dbContext.ClubRoles
                .SingleOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
            
            if (role == null) return new Response<int>("Role not found.");
            if (role.IsSystemRole) return new Response<int>("Cannot delete a system role.");

            _dbContext.ClubRoles.Remove(role);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new Response<int>(role.Id);
        }
    }
}
