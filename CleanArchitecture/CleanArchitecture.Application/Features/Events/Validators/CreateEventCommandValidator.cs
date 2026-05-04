using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Features.Events.Commands.CreateEvent;
using FluentValidation;

namespace CleanArchitecture.Core.Features.Events.Validators
{
    public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
    {
        private readonly IClubRepositoryAsync _clubRepository;

        public CreateEventCommandValidator(IClubRepositoryAsync clubRepository)
        {
            _clubRepository = clubRepository;

            RuleFor(x => x)
                .MustAsync(AllReferencedClubsMustBeActive)
                .WithMessage("Bu etkinlik için seçilen kulüplerden biri kapalı veya uygun değil.");
        }

        private async Task<bool> AllReferencedClubsMustBeActive(CreateEventCommand cmd, CancellationToken cancellationToken)
        {
            foreach (var id in MergeHostClubReferences(cmd))
            {
                if (!await _clubRepository.IsClubActiveAsync(id))
                    return false;
            }
            return true;
        }

        private static IEnumerable<int> MergeHostClubReferences(CreateEventCommand cmd)
        {
            var list = new List<int>();
            if (cmd.HostClubIds != null)
            {
                foreach (var id in cmd.HostClubIds)
                    if (!list.Contains(id))
                        list.Add(id);
            }
            if (cmd.ClubId.HasValue && !list.Contains(cmd.ClubId.Value))
                list.Add(cmd.ClubId.Value);
            return list;
        }
    }
}
