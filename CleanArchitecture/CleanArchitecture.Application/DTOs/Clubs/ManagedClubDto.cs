using System;

namespace CleanArchitecture.Core.DTOs.Clubs
{
    public class ManagedClubDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string LogoUrl { get; set; }
        public string Status { get; set; }
        public decimal? TotalBudget { get; set; }
        public DateTime Created { get; set; }
        public string UserRole { get; set; }
    }
}