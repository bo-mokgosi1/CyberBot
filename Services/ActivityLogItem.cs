using System;

namespace CyberBot.Models
{
    public class ActivityLogItem
    {
        public string Icon { get; set; } = "\uE946";
        public string IconBg { get; set; } = "#1F6FEB33";
        public string Action { get; set; } = "";
        public string Detail { get; set; } = "";
        public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm:ss");
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
