using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Analytics
{
    public class CampusHeatmapDto
    {
        public EventKpiSummary Kpis { get; set; }
        public List<HeatmapRow> Heatmap { get; set; }        // Category × DayOfWeek → event count
        public List<CategoryStat> Categories { get; set; }
        public List<LocationStat> TopLocations { get; set; }
        public List<MonthStat> MonthlyDistribution { get; set; }
    }

    public class EventKpiSummary
    {
        public int TotalEvents { get; set; }
        public int ActiveEvents { get; set; }
        public string TopCategory { get; set; }
        public int TotalCapacity { get; set; }
    }

    public class HeatmapRow
    {
        public string Label { get; set; }   // Category name
        public int[] Values { get; set; }   // index 0=Mon..6=Sun, value = event count
        public int Peak { get; set; }
    }

    public class CategoryStat
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string Color { get; set; }
    }

    public class LocationStat
    {
        public string Location { get; set; }
        public int Count { get; set; }
    }

    public class MonthStat
    {
        public string Month { get; set; }
        public int Count { get; set; }
    }
}
