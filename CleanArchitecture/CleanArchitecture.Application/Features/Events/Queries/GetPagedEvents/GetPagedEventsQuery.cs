using AutoMapper;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Events.Common;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
        private readonly IMapper _mapper;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetPagedEventsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            IAuthenticatedUserService authenticatedUserService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _authenticatedUserService = authenticatedUserService;
            _userManager = userManager;
        }

        public async Task<PagedResponse<EventViewModel>> Handle(GetPagedEventsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Events
                .Include(e => e.EventClubs)
                .ThenInclude(ec => ec.Club)
                .AsNoTracking()
                .AsQueryable();

            if (request.ClubId.HasValue)
                query = query.Where(e => e.EventClubs.Any(ec => ec.ClubId == request.ClubId.Value));

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
                    e.EventClubs.Any(ec => ec.Club != null && EF.Functions.ILike(ec.Club.Name, search)));
            }

            // Visibility filter — kullanıcının göremeyeceği etkinlikleri sayım dahil hiçbir yerde sızdırma
            query = await EventVisibilityFilter.ApplyAsync(
                query, _context, _userManager, _authenticatedUserService.UserId, cancellationToken);

            var totalCount = await query.CountAsync(cancellationToken);

            query = request.SortBy?.ToLower() switch
            {
                "title" => request.IsDescending ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
                "location" => request.IsDescending ? query.OrderByDescending(e => e.Location) : query.OrderBy(e => e.Location),
                "clubname" => request.IsDescending
                    ? query.OrderByDescending(e => e.EventClubs.OrderBy(ec => ec.SortOrder).Select(ec => ec.Club.Name).FirstOrDefault())
                    : query.OrderBy(e => e.EventClubs.OrderBy(ec => ec.SortOrder).Select(ec => ec.Club.Name).FirstOrDefault()),
                "enddate" => request.IsDescending ? query.OrderByDescending(e => e.EndDate) : query.OrderBy(e => e.EndDate),
                _ => request.IsDescending ? query.OrderByDescending(e => e.StartDate) : query.OrderBy(e => e.StartDate)
            };

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var data = _mapper.Map<List<EventViewModel>>(items);

            var userId = _authenticatedUserService.UserId;
            if (!string.IsNullOrEmpty(userId) && data.Count > 0)
            {
                var ids = data.Select(v => v.Id).ToList();
                var registeredIds = await _context.EventAttendees
                    .Where(a => a.UserId == userId && ids.Contains(a.EventId))
                    .Select(a => a.EventId)
                    .ToListAsync(cancellationToken);
                var registeredSet = new HashSet<int>(registeredIds);
                foreach (var vm in data)
                    vm.IsRegistered = registeredSet.Contains(vm.Id);
            }

            return new PagedResponse<EventViewModel>(data, request.PageNumber, request.PageSize, totalCount);
        }
    }
}
