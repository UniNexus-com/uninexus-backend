using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Inventory.Commands.ReturnAsset
{
    public class ReturnAssetCommand : IRequest<Response<int>>
    {
        public int AssetId { get; set; }
    }

    public class ReturnAssetCommandHandler : IRequestHandler<ReturnAssetCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUser;
        private readonly IClubRepositoryAsync _clubRepository;

        public ReturnAssetCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUser, IClubRepositoryAsync clubRepository)
        {
            _context = context;
            _authenticatedUser = authenticatedUser;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(ReturnAssetCommand request, CancellationToken cancellationToken)
        {
            var requesterId = _authenticatedUser.UserId;
            if (string.IsNullOrEmpty(requesterId))
                throw new ApiException("You must be logged in to return items.");

            // 1. Find the active loan for this asset
            var loan = await _context.AssetLoans
                .FirstOrDefaultAsync(l => l.AssetId == request.AssetId
                                       && (l.Status == "Active" || l.Status == "Overdue"),
                                       cancellationToken);

            if (loan == null)
                throw new ApiException("No active loan found for this item.");

            // 2. Authorization: Check if user is borrower OR has Manage Assets privilege for the club
            if (loan.UserId != requesterId)
            {
                var assetObj = await _context.Assets.FindAsync(new object[] { request.AssetId }, cancellationToken);
                if (assetObj == null || !await _clubRepository.HasPrivilegeInClubAsync(assetObj.ClubId.Value, requesterId, "Manage Assets"))
                    throw new ApiException("Authorization Error: You do not have permission to return this item on behalf of another user.");
            }

            // 2. Update loan record
            loan.ReturnedAt = DateTime.UtcNow;
            loan.Status = "Returned";
            _context.AssetLoans.Update(loan);

            // 3. Update asset status back to available
            var asset = await _context.Assets.FindAsync(new object[] { request.AssetId }, cancellationToken);
            if (asset != null)
            {
                asset.Status = "AVAILABLE";
                _context.Assets.Update(asset);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var wasOverdue = loan.DueDate < loan.ReturnedAt;
            var message = wasOverdue
                ? "Item returned (was overdue). Your borrowing privileges have been restored."
                : "Item returned successfully.";

            return new Response<int>(loan.Id, message: message);
        }
    }
}
