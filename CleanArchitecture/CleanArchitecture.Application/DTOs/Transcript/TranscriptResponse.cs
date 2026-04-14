using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.DTOs.Transcript
{
    public class TranscriptResponse
    {
        public string StudentName { get; set; }
        public string StudentNumber { get; set; }
        public int TotalPoints { get; set; }
        public List<TranscriptEventItem> Activities { get; set; }
    }
}
