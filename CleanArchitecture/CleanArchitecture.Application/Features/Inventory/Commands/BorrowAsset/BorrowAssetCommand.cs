using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Inventory.Commands.BorrowAsset
{
    public class BorrowAssetCommand : IRequest<Response<int>>
    {
        public int AssetId { get; set; }
    }

    public class BorrowAssetCommandHandler : IRequestHandler<BorrowAssetCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUser;

        public BorrowAssetCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUser)
        {
            _context = context;
            _authenticatedUser = authenticatedUser;
        }

        public async Task<Response<int>> Handle(BorrowAssetCommand request, CancellationToken cancellationToken)
        {
            var userId = _authenticatedUser.UserId;
            if (string.IsNullOrEmpty(userId))
                throw new ApiException("You must be logged in to borrow items.");

            // 1. Check Inventory Lock – user must not have any overdue loans
            var hasOverdue = await _context.AssetLoans
                .AnyAsync(l => l.UserId == userId && l.Status == "Overdue", cancellationToken);

            if (hasOverdue)
                throw new ApiException("Inventory Lock: You have overdue items. Return them before borrowing new equipment.");

            var hasActiveLate = await _context.AssetLoans
                .AnyAsync(l => l.UserId == userId && l.Status == "Active" && l.DueDate < DateTime.UtcNow, cancellationToken);

            if (hasActiveLate)
                throw new ApiException("Inventory Lock: You have overdue items. Return them before borrowing new equipment.");

            // 2. Check asset exists and is available
            var asset = await _context.Assets.FindAsync(new object[] { request.AssetId }, cancellationToken);
            if (asset == null)
                throw new EntityNotFoundException("Asset", request.AssetId);

            if (asset.Status != "AVAILABLE")
                throw new ApiException($"This item is not available for borrowing. Current status: {asset.Status}");

            // 3. Create loan record
            var loan = new AssetLoan
            {
                AssetId = request.AssetId,
                UserId = userId,
                BorrowedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7), // Default: 7 days
                Status = "Active"
            };

            await _context.AssetLoans.AddAsync(loan, cancellationToken);

            // 4. Update asset status
            asset.Status = "ON_LOAN";
            _context.Assets.Update(asset);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<int>(loan.Id, message: $"'{asset.Name}' borrowed successfully. Due date: {loan.DueDate:dd MMM yyyy}");
        }
    }
}
