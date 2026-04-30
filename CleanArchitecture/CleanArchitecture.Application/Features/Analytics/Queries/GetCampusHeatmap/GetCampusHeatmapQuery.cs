using CleanArchitecture.Core.DTOs.Analytics;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Features.Analytics.Queries.GetCampusHeatmap
{
    public class GetCampusHeatmapQuery : IRequest<Response<CampusHeatmapDto>>
    {
    }

    public class GetCampusHeatmapQueryHandler : IRequestHandler<GetCampusHeatmapQuery, Response<CampusHeatmapDto>>
    {
        private readonly IGenericRepositoryAsync<Entities.Event> _eventRepository;

        private static readonly string[] CategoryColors =
            ["#4A90D9", "#F5A623", "#10b981", "#8b5cf6", "#ec4899", "#f43f5e", "#06b6d4", "#a16207"];

        public GetCampusHeatmapQueryHandler(IGenericRepositoryAsync<Entities.Event> eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<Response<CampusHeatmapDto>> Handle(GetCampusHeatmapQuery request, CancellationToken cancellationToken)
        {
            var events = (await _eventRepository.GetAllAsync()).ToList();

            // KPIs
            var kpis = new EventKpiSummary
            {
                TotalEvents = events.Count,
                ActiveEvents = events.Count(e => e.IsActive),
                TopCategory = events
                    .Where(e => !string.IsNullOrEmpty(e.Category))
                    .GroupBy(e => e.Category)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "-",
                TotalCapacity = events.Sum(e => e.Capacity ?? 0)
            };

            // Heatmap: Category × DayOfWeek (Mon=0..Sun=6)
            var categories = events
                .Where(e => !string.IsNullOrEmpty(e.Category))
                .GroupBy(e => e.Category)
                .OrderByDescending(g => g.Count())
                .Take(6)
                .ToList();

            var heatmap = categories.Select((g, idx) =>
            {
                var values = new int[7];
                foreach (var ev in g)
                {
                    int dayIdx = ev.StartDate.DayOfWeek == DayOfWeek.Sunday
                        ? 6
                        : (int)ev.StartDate.DayOfWeek - 1;
                    values[dayIdx]++;
                }
                return new HeatmapRow
                {
                    Label = g.Key,
                    Values = values,
                    Peak = values.Max()
                };
            }).ToList();

            // Category stats
            var categoryStats = categories.Select((g, idx) => new CategoryStat
            {
                Name = g.Key,
                Count = g.Count(),
                Color = CategoryColors[idx % CategoryColors.Length]
            }).ToList();

            // Top locations
            var topLocations = events
                .Where(e => !string.IsNullOrEmpty(e.Location))
                .GroupBy(e => e.Location)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new LocationStat { Location = g.Key, Count = g.Count() })
                .ToList();

            // Monthly distribution (last 6 months)
            var monthlyDistribution = events
                .GroupBy(e => new { e.StartDate.Year, e.StartDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .TakeLast(6)
                .Select(g => new MonthStat
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yy"),
                    Count = g.Count()
                })
                .ToList();

            var dto = new CampusHeatmapDto
            {
                Kpis = kpis,
                Heatmap = heatmap,
                Categories = categoryStats,
                TopLocations = topLocations,
                MonthlyDistribution = monthlyDistribution
            };

            return new Response<CampusHeatmapDto>(dto);
        }
    }
}
