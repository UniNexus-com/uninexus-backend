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
        Task RemoveMemberAsync(int clubId, string userId);
        Task<bool> IsClubMemberAsync(int clubId, string userId);
        Task<bool> HasPendingJoinRequestAsync(int clubId, string userId);
        Task<IReadOnlyList<Club>> GetPresidentClubsAsync(string userId);
        Task<bool> IsPresidentOfClubAsync(int clubId, string userId);
        Task<bool> HasAuthorityInClubAsync(int clubId, string userId);
        Task<bool> HasPrivilegeInClubAsync(int clubId, string userId, string privilegeName);
        Task<ClubUserPermissionsDto> GetClubUserPermissionsAsync(int clubId, string userId);
    }
}
