using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.RemoveMember
{
    public class RemoveMemberCommand : IRequest<Response<string>>
    {
        public int ClubId { get; set; }
        public string UserId { get; set; }
    }

    public class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Response<string>>
    {
        private readonly IClubRepositoryAsync _clubRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public RemoveMemberCommandHandler(IClubRepositoryAsync clubRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _clubRepository = clubRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<string>> Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
        {
            if (!await _clubRepository.HasPrivilegeInClubAsync(request.ClubId, _authenticatedUserService.UserId, "Manage Members"))
                throw new ApiException("You do not have permission to manage members in this club.");

            try
            {
                await _clubRepository.RemoveMemberAsync(request.ClubId, request.UserId);
                return new Response<string>(request.UserId, "Member removed successfully.");
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                throw new ApiException(ex.Message);
            }
        }
    }
}
