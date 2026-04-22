using CleanArchitecture.Core.Features.Clubs.Commands.AcceptJoinRequest;
using CleanArchitecture.Core.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Validators
{
    public class AcceptJoinRequestCommandValidator : AbstractValidator<AcceptJoinRequestCommand>
    {
        private readonly IClubRepositoryAsync _clubRepository;

        public AcceptJoinRequestCommandValidator(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;

            RuleFor(p => p.Id)
                .NotEmpty().WithMessage("Request Id is required.")
                .MustAsync(BelongToActiveClub)
                .WithMessage("The club associated with this request is suspended or closed. You cannot accept new members.");
        }
        private async Task<bool> BelongToActiveClub(int joinRequestId, CancellationToken cancellationToken)
        {
            var joinRequest = await _clubRepository.GetJoinRequestByIdAsync(joinRequestId);

            if (joinRequest == null) return true;

            return await _clubRepository.IsClubActiveAsync(joinRequest.ClubId);
        }
    }
}
