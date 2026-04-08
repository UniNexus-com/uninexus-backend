using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Roles.Commands.UpdateMemberRole
{
    public class UpdateMemberRoleCommand : IRequest<Response<string>>
    {
        public int ClubId { get; set; }
        public string UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateMemberRoleCommandHandler : IRequestHandler<UpdateMemberRoleCommand, Response<string>>
    {
        private readonly IApplicationDbContext _dbContext;

        public UpdateMemberRoleCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response<string>> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
        {
            var membership = await _dbContext.UserClubs
                .SingleOrDefaultAsync(uc => uc.UserId == request.UserId && uc.ClubId == request.ClubId, cancellationToken);
            
            if (membership == null) return new Response<string>("Membership not found.");
            
            membership.ClubRoleId = request.RoleId;
            
            _dbContext.UserClubs.Update(membership);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new Response<string>(request.UserId);
        }
    }
}
