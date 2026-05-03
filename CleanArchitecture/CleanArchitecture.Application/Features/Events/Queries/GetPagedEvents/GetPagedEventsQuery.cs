using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Events.Queries.GetPagedEvents
{
    public class GetPagedEventsQuery : IRequest<PagedResponse<EventViewModel>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SearchValue { get; set; }
        public int? ClubId { get; set; }
        public string Category { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "StartDate";
        public bool IsDescending { get; set; } = true;
    }

    public class GetPagedEventsQueryHandler : IRequestHandler<GetPagedEventsQuery, PagedResponse<EventViewModel>>
    {
        private readonly IApplicationDbContext _context;

        public GetPagedEventsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<EventViewModel>> Handle(GetPagedEventsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Events
                .Include(e => e.Club)
                .AsNoTracking()
                .AsQueryable();

            if (request.ClubId.HasValue)
                query = query.Where(e => e.ClubId == request.ClubId.Value);

            if (!string.IsNullOrEmpty(request.Category))
                query = query.Where(e => e.Category == request.Category);

            if (request.IsActive.HasValue)
                query = query.Where(e => e.IsActive == request.IsActive.Value);

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                var search = $"%{request.SearchValue}%";
                query = query.Where(e =>
                    EF.Functions.ILike(e.Title, search) ||
                    (e.Location != null && EF.Functions.ILike(e.Location, search)) ||
                    (e.Club != null && EF.Functions.ILike(e.Club.Name, search)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "title"    => request.IsDescending ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
                "location" => request.IsDescending ? query.OrderByDescending(e => e.Location) : query.OrderBy(e => e.Location),
                "clubname" => request.IsDescending ? query.OrderByDescending(e => e.Club.Name) : query.OrderBy(e => e.Club.Name),
                "enddate"  => request.IsDescending ? query.OrderByDescending(e => e.EndDate) : query.OrderBy(e => e.EndDate),
                _          => request.IsDescending ? query.OrderByDescending(e => e.StartDate) : query.OrderBy(e => e.StartDate)
            };

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new EventViewModel
                {
                    Id             = e.Id,
                    Title          = e.Title,
                    Description    = e.Description,
                    StartDate      = e.StartDate,
                    EndDate        = e.EndDate,
                    Location       = e.Location,
                    IsActive       = e.IsActive,
                    Category       = e.Category,
                    Visibility     = e.Visibility,
                    Capacity       = e.Capacity,
                    Requirements   = e.Requirements,
                    RequireApproval = e.RequireApproval,
                    ClubId         = e.ClubId,
                    ClubName       = e.Club != null ? e.Club.Name : null
                })
                .ToListAsync(cancellationToken);

            return new PagedResponse<EventViewModel>(data, request.PageNumber, request.PageSize, totalCount);
        }
    }
}
