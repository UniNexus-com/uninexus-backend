using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces
{
    public interface INotificationService
    {
        Task BroadcastMessageAsync(string title, string message);

        Task BroadcastToGroupAsync(string groupName, string title, string message);
    }
}
