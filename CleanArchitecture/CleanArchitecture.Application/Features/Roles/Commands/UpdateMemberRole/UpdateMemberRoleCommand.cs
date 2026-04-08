using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
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
        private readonly IGenericRepositoryAsync<UserClub> _userClubRepo;

        public UpdateMemberRoleCommandHandler(IGenericRepositoryAsync<UserClub> userClubRepo)
        {
            _userClubRepo = userClubRepo;
        }

        public async Task<Response<string>> Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
        {
            var all = await _userClubRepo.GetAllAsync();
            var membership = all.FirstOrDefault(uc => uc.UserId == request.UserId && uc.ClubId == request.ClubId);
            if (membership == null) return new Response<string>("Membership not found.");
            membership.ClubRoleId = request.RoleId;
            await _userClubRepo.UpdateAsync(membership);
            return new Response<string>(request.UserId);
        }
    }
}
