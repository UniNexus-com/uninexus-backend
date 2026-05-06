using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.DTOs.Transcript
{
    public class TranscriptEventItem
    {
        public string EventName { get; set; }
        public string Date { get; set; }
        public int Points { get; set; }
        public string Category { get; set; } = "Event";
    }
}
