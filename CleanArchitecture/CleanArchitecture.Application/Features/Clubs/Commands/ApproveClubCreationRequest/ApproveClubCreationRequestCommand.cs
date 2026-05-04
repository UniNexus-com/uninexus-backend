using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Clubs.Commands.ApproveClubCreationRequest
{
    public class ApproveClubCreationRequestCommand : IRequest<Response<int>>
    {
        public int RequestId { get; set; }
    }

    public class ApproveClubCreationRequestCommandHandler : IRequestHandler<ApproveClubCreationRequestCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAccountService _accountService;

        public ApproveClubCreationRequestCommandHandler(
            IApplicationDbContext context,
            IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        public async Task<Response<int>> Handle(ApproveClubCreationRequestCommand request, CancellationToken cancellationToken)
        {
            // 1. Load and validate request
            var creationRequest = await _context.ClubCreationRequests
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

            if (creationRequest == null) throw new ApiException("No request found.");
            if (creationRequest.Status != "PENDING") throw new ApiException("This request has already been processed.");

            // 2. Create the club
            var newClub = new Club
            {
                Name = creationRequest.Name,
                Description = creationRequest.Description,
                Category = creationRequest.Category,
                IsActive = true,
                Status = "ACTIVE",
                TotalBudget = 0
            };
            await _context.Clubs.AddAsync(newClub, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken); // commit to get newClub.Id

            // 3. Assign CLUB_LEADER system role + create President UserClub record
            //    (ChangeUserRoleAsync handles both Identity role and UserClub creation)
            await _accountService.ChangeUserRoleAsync(
                creationRequest.RequesterUserId,
                "CLUB_LEADER",
                newClub.Id);

            // 4. Add all supporters as club members
            var supporterUserIds = await _context.ClubCreationRequestSupporters
                .Where(s => s.ClubCreationRequestId == creationRequest.Id)
                .Select(s => s.UserId)
                .ToListAsync(cancellationToken);

            foreach (var supporterId in supporterUserIds)
            {
                var alreadyMember = await _context.UserClubs
                    .AnyAsync(uc => uc.ClubId == newClub.Id && uc.UserId == supporterId, cancellationToken);

                if (!alreadyMember)
                {
                    await _context.UserClubs.AddAsync(new UserClub
                    {
                        UserId = supporterId,
                        ClubId = newClub.Id,
                        JoinDate = DateTime.UtcNow,
                        IsActive = true
                        // ClubRoleId intentionally omitted — DB trigger assigns default Member role
                    }, cancellationToken);
                }
            }

            // 5. Mark request as approved
            creationRequest.Status = "APPROVED";
            _context.ClubCreationRequests.Update(creationRequest);

            var creatorUser = await _context.Set<ApplicationUser>().FindAsync(new object[] { creationRequest.RequesterUserId }, cancellationToken);
            if (creatorUser != null)
            {
                creatorUser.ScoreWalletBalance += 1000;
                creatorUser.TotalScore += 1000;
                _context.Set<ApplicationUser>().Update(creatorUser);
            }

            // 6. Club-internal announcement
            await _context.Announcements.AddAsync(new Announcement
            {
                Title = $"{newClub.Name} Kulübü Aktif!",
                Message = $"Kulübümüz SKS tarafından resmi olarak onaylandı ve aktif hale geldi. Topluluğa hoş geldiniz!",
                Priority = AnnouncementPriority.High,
                ClubId = newClub.Id
            }, cancellationToken);

            // 7. Global broadcast announcement
            var requester = await _context.Set<ApplicationUser>()
                .Where(u => u.Id == creationRequest.RequesterUserId)
                .Select(u => new { u.FullName })
                .FirstOrDefaultAsync(cancellationToken);

            var leaderName = requester?.FullName ?? "Bir öğrenci";

            await _context.Announcements.AddAsync(new Announcement
            {
                Title = $"Yeni Kulüp Kuruldu: {newClub.Name}",
                Message = $"{leaderName} önderliğinde \"{newClub.Name}\" kulübü resmi olarak kurulmuştur ve artık hizmet vermektedir!",
                Priority = AnnouncementPriority.Medium,
                ClubId = null
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<int>(newClub.Id, $"{newClub.Name} kulübü başarıyla oluşturuldu ve sisteme eklendi.");
        }
    }
}
