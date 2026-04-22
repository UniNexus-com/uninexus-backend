using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.DTOs.LeaderbordUserDto
{
    public class LeaderboardUserDto
    {
        public int Rank { get; set; }
        public string UserId { get; set; }
        public string StudentNumber { get; set; }
        public string FullName { get; set; }
        public int ScoreWalletBalance { get; set; }
    }
}
