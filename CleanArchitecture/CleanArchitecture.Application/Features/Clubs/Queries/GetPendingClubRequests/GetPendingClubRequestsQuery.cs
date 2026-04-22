using AutoMapper;
using CleanArchitecture.Core.DTOs.Clubs;
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

namespace CleanArchitecture.Core.Features.Clubs.Queries.GetPendingClubRequests
{
    public class GetPendingClubRequestsQuery : IRequest<Response<IEnumerable<ClubCreationRequestDto>>>
    {
    }

    public class GetPendingClubRequestsQueryHandler : IRequestHandler<GetPendingClubRequestsQuery, Response<IEnumerable<ClubCreationRequestDto>>>
    {
        private readonly IGenericRepositoryAsync<ClubCreationRequest> _requestRepo;
        private readonly IMapper _mapper;

        public GetPendingClubRequestsQueryHandler(IGenericRepositoryAsync<ClubCreationRequest> requestRepo, IMapper mapper)
        {
            _requestRepo = requestRepo;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ClubCreationRequestDto>>> Handle(GetPendingClubRequestsQuery request, CancellationToken cancellationToken)
        {
            var pendingRequests = await _requestRepo.GetPagedReponseAsync(1, 100);

            var filtered = new List<ClubCreationRequest>();
            foreach (var item in pendingRequests)
            {
                if (item.Status == "PENDING") filtered.Add(item);
            }

            var dto = _mapper.Map<IEnumerable<ClubCreationRequestDto>>(filtered);
            return new Response<IEnumerable<ClubCreationRequestDto>>(dto);
        }
    }
}
