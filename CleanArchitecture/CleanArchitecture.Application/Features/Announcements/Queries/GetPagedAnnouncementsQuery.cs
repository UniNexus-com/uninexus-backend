using CleanArchitecture.Core.DTOs.Announcement;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Announcements.Queries
{
    public class GetPagedAnnouncementsQuery : IRequest<PagedResponse<AnnouncementResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SearchValue { get; set; }
        public string Priority { get; set; }
        // null = all, true = global only (ClubId == null), false = club only
        public bool? IsGlobal { get; set; }
        public string SortBy { get; set; } = "Created";
        public bool IsDescending { get; set; } = true;
    }

    public class GetPagedAnnouncementsQueryHandler : IRequestHandler<GetPagedAnnouncementsQuery, PagedResponse<AnnouncementResponse>>
    {
        private readonly IApplicationDbContext _context;

        public GetPagedAnnouncementsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<AnnouncementResponse>> Handle(GetPagedAnnouncementsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Announcements.AsNoTracking().AsQueryable();

            if (request.IsGlobal.HasValue)
                query = request.IsGlobal.Value
                    ? query.Where(a => a.ClubId == null)
                    : query.Where(a => a.ClubId != null);

            if (!string.IsNullOrEmpty(request.Priority))
                query = query.Where(a => a.Priority.ToString() == request.Priority);

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = $"%{request.SearchValue}%";
                query = query.Where(a =>
                    EF.Functions.ILike(a.Title, search) ||
                    EF.Functions.ILike(a.Message, search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "title"    => request.IsDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
                "priority" => request.IsDescending ? query.OrderByDescending(a => a.Priority) : query.OrderBy(a => a.Priority),
                _          => request.IsDescending ? query.OrderByDescending(a => a.Created) : query.OrderBy(a => a.Created)
            };

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AnnouncementResponse
                {
                    Id        = a.Id,
                    CreatedAt = a.Created,
                    Title     = a.Title,
                    Message   = a.Message,
                    Priority  = a.Priority
                })
                .ToListAsync(cancellationToken);

            return new PagedResponse<AnnouncementResponse>(data, request.PageNumber, request.PageSize, totalCount);
        }
    }
}
