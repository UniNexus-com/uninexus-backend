using System.Collections.Generic;

namespace CleanArchitecture.Core.DTOs.Account
{
    public class GetPagedUsersRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SearchValue { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Statuses { get; set; }
        public string SortBy { get; set; } = "FullName";
        public bool IsDescending { get; set; } = false;
    }
}
