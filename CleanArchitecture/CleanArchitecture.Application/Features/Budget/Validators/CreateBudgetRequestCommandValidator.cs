using CleanArchitecture.Core.Features.Finance.Commands.CreateBudgetRequest;
using CleanArchitecture.Core.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Budget.Validators
{
    public class CreateBudgetRequestCommandValidator : AbstractValidator<CreateBudgetRequestCommand>
    {
        private readonly IClubRepositoryAsync _clubRepository;

        public CreateBudgetRequestCommandValidator(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;

            When(p => p.ClubId.HasValue, () =>
            {
                RuleFor(p => p.ClubId.Value)
                    .MustAsync(BeActiveClub)
                    .WithMessage("This club is currently suspended or closed. You cannot create budget requests.");
            });
        }

        private async Task<bool> BeActiveClub(int clubId, CancellationToken cancellationToken)
        {
            return await _clubRepository.IsClubActiveAsync(clubId);
        }
    }
}
