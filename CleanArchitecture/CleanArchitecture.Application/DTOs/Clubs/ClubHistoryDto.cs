using System;
using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class ClubHistoryDto
    {
        public List<HistoryPointDto> Points { get; set; } = new List<HistoryPointDto>();
    }

    public class HistoryPointDto
    {
        public DateTime Date { get; set; }
        public int MemberCount { get; set; }
        public int EventCount { get; set; }
        public decimal TotalBudgetUsed { get; set; }
    }
}
