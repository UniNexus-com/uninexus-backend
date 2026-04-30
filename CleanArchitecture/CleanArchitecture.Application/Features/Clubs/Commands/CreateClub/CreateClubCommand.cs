using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.CreateClub
{
    public class CreateClubCommand : IRequest<Response<int>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string Status { get; set; } = "ACTIVE";
        public decimal? TotalBudget { get; set; } = 0;
    }

    public class CreateClubCommandHandler : IRequestHandler<CreateClubCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<Club> _clubRepo;

        public CreateClubCommandHandler(IGenericRepositoryAsync<Club> clubRepo)
        {
            _clubRepo = clubRepo;
        }

        public async Task<Response<int>> Handle(CreateClubCommand request, CancellationToken cancellationToken)
        {
            var club = new Club
            {
                Name        = request.Name,
                Description = request.Description,
                LogoUrl     = request.LogoUrl,
                IsActive    = request.Status == "ACTIVE",
                Status      = request.Status,
                TotalBudget = request.TotalBudget ?? 0,
            };

            await _clubRepo.AddAsync(club);

            return new Response<int>(club.Id, $"Kulüp '{club.Name}' başarıyla oluşturuldu.");
        }
    }
}
