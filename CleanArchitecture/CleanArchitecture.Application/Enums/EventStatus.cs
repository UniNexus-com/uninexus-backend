using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Enums
{
    public enum EventStatus
    {
        Draft = 1,
        PendingSKSApproval = 2,
        Approved = 3,
        Rejected = 4,
        Cancelled = 5
    }
}
