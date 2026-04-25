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

        public ReturnAssetCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUser)
        {
            _context = context;
            _authenticatedUser = authenticatedUser;
        }

        public async Task<Response<int>> Handle(ReturnAssetCommand request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUser.UserId;
            if (string.IsNullOrEmpty(userId))
                throw new ApiException("You must be logged in to return items.");

            // 1. Find the active loan for this asset by this user
            var loan = await _context.AssetLoans
                .FirstOrDefaultAsync(l => l.AssetId == request.AssetId
                                       && l.UserId == userId
                                       && (l.Status == "Active" || l.Status == "Overdue"),
                                       cancellationToken);

            if (loan == null)
                throw new ApiException("No active loan found for this item.");

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
