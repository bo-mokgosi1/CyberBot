using System;
using System.Collections.ObjectModel;
using System.Linq;
using CyberBot.Models;

namespace CyberBot.Services
{
    public class ActivityLogService
    {
        private readonly ObservableCollection<ActivityLogItem> logItems = new();

        public ObservableCollection<ActivityLogItem> LogItems => logItems;
        public void Log(string icon, string iconBg, string action, string detail)
        {
            LogItems.Insert(0, new ActivityLogItem
            {
                Icon = icon,
                IconBg = iconBg,
                Action = action,
                Detail = detail,
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                CreatedAt = DateTime.Now
            });
            while (LogItems.Count > 500) LogItems.RemoveAt(LogItems.Count - 1);
        }

        public void Clear() => LogItems.Clear();
    }
}