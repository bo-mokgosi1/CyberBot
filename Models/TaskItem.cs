using System;
using System.Windows;

namespace CyberBot.Models
{
    public class TaskItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? ReminderDate { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Segoe MDL2 Assets icon codes
        public string StatusIcon => Status == "Complete" ? "\uE930" : "\uE762";
        public string StatusIconColor => Status == "Complete" ? "#3FB950" : "#8B949E";
        public string TitleColor => Status == "Complete" ? "#8B949E" : "White";
        public string StatusBg => Status == "Complete" ? "#238636" : "#1F6FEB";

        public string ReminderText => ReminderDate.HasValue
            ? $"Reminder: {ReminderDate.Value:dd MMM yyyy}"
            : "";

        public Visibility ReminderVisibility =>
            ReminderDate.HasValue ? Visibility.Visible : Visibility.Collapsed;
    }
}
