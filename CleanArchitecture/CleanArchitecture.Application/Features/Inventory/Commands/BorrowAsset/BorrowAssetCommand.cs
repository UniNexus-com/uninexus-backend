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
        public string UserId { get; set; }
    }

    public class BorrowAssetCommandHandler : IRequestHandler<BorrowAssetCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuthenticatedUserService _authenticatedUser;
        private readonly IClubRepositoryAsync _clubRepository;

        public BorrowAssetCommandHandler(IApplicationDbContext context, IAuthenticatedUserService authenticatedUser, IClubRepositoryAsync clubRepository)
        {
            _context = context;
            _authenticatedUser = authenticatedUser;
            _clubRepository = clubRepository;
        }

        public async Task<Response<int>> Handle(BorrowAssetCommand request, CancellationToken cancellationToken)
        {
            var requesterId = _authenticatedUser.UserId;
            if (string.IsNullOrEmpty(requesterId))
                throw new ApiException("You must be logged in to borrow items.");

            var userId = request.UserId ?? requesterId;

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

            // Authorization: If borrowing for someone else, must have Manage Assets privilege
            if (userId != requesterId)
            {
                if (!await _clubRepository.HasPrivilegeInClubAsync(asset.ClubId.Value, requesterId, "Manage Assets"))
                    throw new ApiException("Authorization Error: You do not have permission to borrow items on behalf of others.");
            }

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

            var loanUser = await _context.Set<ApplicationUser>().FindAsync(new object[] { userId }, cancellationToken);
            if (loanUser != null)
            {
                loanUser.ScoreWalletBalance += 50;
                loanUser.TotalScore += 50;
                _context.Set<ApplicationUser>().Update(loanUser);
            }

            // 4. Update asset status
            asset.Status = "ON_LOAN";
            _context.Assets.Update(asset);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<int>(loan.Id, message: $"'{asset.Name}' borrowed to {userId}. Due date: {loan.DueDate:dd MMM yyyy}");
        }
    }
}
