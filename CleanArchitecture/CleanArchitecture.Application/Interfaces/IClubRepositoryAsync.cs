using CleanArchitecture.Core.DTOs.Clubs;
using CleanArchitecture.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IClubRepositoryAsync : IGenericRepositoryAsync<Club>
    {
        Task<IReadOnlyList<ClubMemberDto>> GetClubMembersAsync(int clubId);
        Task<IReadOnlyList<ClubJoinRequestDto>> GetClubJoinRequestsAsync(int clubId);
        Task<IReadOnlyList<Club>> GetManagedClubsAsync(string userId);
        Task<MemberDetailsDto> GetClubMemberDetailsAsync(int clubId, string userId);
        Task<ClubJoinRequest> GetJoinRequestByIdAsync(int requestId);
        Task UpdateJoinRequestAsync(ClubJoinRequest joinRequest);
        Task<ClubStatsDto> GetClubStatsAsync(int clubId);
    }
}
