using CleanArchitecture.Core.Features.Announcements.Commands;
using CleanArchitecture.Core.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Announcements.Validators
{
    public class CreateClubAnnouncementCommandValidator : AbstractValidator<CreateClubAnnouncementCommand>
    {
        private readonly IClubRepositoryAsync _clubRepository;

        public CreateClubAnnouncementCommandValidator(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;

            RuleFor(p => p.ClubId)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .MustAsync(BeActiveClub)
                .WithMessage("This club is currently suspended or closed. You cannot broadcast announcements.");
        }

        private async Task<bool> BeActiveClub(int clubId, CancellationToken cancellationToken)
        {
            return await _clubRepository.IsClubActiveAsync(clubId);
        }
    }
}
