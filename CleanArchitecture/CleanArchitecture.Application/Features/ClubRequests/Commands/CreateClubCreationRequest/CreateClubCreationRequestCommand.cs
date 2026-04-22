using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.ClubRequests.Commands.CreateClubCreationRequest
{
    public class CreateClubCreationRequestCommand : IRequest<Response<int>>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string AdvisorName { get; set; }
    }

    public class CreateClubCreationRequestCommandHandler : IRequestHandler<CreateClubCreationRequestCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<ClubCreationRequest> _repository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public CreateClubCreationRequestCommandHandler(
            IGenericRepositoryAsync<ClubCreationRequest> repository,
            IAuthenticatedUserService authenticatedUserService)
        {
            _repository = repository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<int>> Handle(CreateClubCreationRequestCommand request, CancellationToken cancellationToken)
        {
            var entity = new ClubCreationRequest
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                AdvisorName = request.AdvisorName,
                RequesterUserId = _authenticatedUserService.UserId,
                Status = "PENDING"
            };

            await _repository.AddAsync(entity);
            return new Response<int>(entity.Id, "Your request to establish a club has been successfully received and submitted for managerial approval.");
        }
    }
}
