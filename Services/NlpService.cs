using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CyberBot.Models;

namespace CyberBot.Services
{
    public class NlpService
    {
        public string UserName { get; set; } = "User";

        private readonly ActivityLogService _log;
        private readonly Random _rng;

        private readonly Dictionary<string, string> _intents;
        private readonly Dictionary<string, string[]> _tips;

        public NlpService(ActivityLogService log)
        {
            _log = log;
            _rng = new Random();

            _intents = new Dictionary<string, string>
            {
                { "add task", "ADD_TASK" },
                { "create task", "ADD_TASK" },
                { "new task", "ADD_TASK" },
                { "remind me", "SET_REMINDER" },
                { "set reminder", "SET_REMINDER" },
                { "show tasks", "VIEW_TASKS" },
                { "my tasks", "VIEW_TASKS" },
                { "list tasks", "VIEW_TASKS" },
                { "start quiz", "START_QUIZ" },
                { "take quiz", "START_QUIZ" },
                { "quiz me", "START_QUIZ" },
                { "test my knowledge", "START_QUIZ" },
                { "play quiz", "START_QUIZ" },
                { "phishing", "TOPIC_PHISHING" },
                { "fake email", "TOPIC_PHISHING" },
                { "password", "TOPIC_PASSWORD" },
                { "passphrase", "TOPIC_PASSWORD" },
                { "two factor", "TOPIC_2FA" },
                { "2fa", "TOPIC_2FA" },
                { "authenticator", "TOPIC_2FA" },
                { "malware", "TOPIC_MALWARE" },
                { "virus", "TOPIC_MALWARE" },
                { "ransomware", "TOPIC_MALWARE" },
                { "vpn", "TOPIC_VPN" },
                { "virtual private network", "TOPIC_VPN" },
                { "scam", "TOPIC_SCAM" },
                { "fraud", "TOPIC_SCAM" },
                { "social engineering", "TOPIC_SCAM" },
                { "privacy", "TOPIC_PRIVACY" },
                { "tracking", "TOPIC_PRIVACY" },
                { "firewall", "TOPIC_FIREWALL" },
                { "activity log", "VIEW_LOG" },
                { "show log", "VIEW_LOG" },
                { "what have you done", "VIEW_LOG" },
                { "recent actions", "VIEW_LOG" },
                { "history", "VIEW_LOG" },
                { "hello", "GREETING" },
                { "hi", "GREETING" },
                { "hey", "GREETING" },
                { "good morning", "GREETING" },
                { "thanks", "THANKS" },
                { "thank you", "THANKS" },
                { "help", "HELP" },
                { "what can you do", "HELP" }
            };

            _tips = new Dictionary<string, string[]>
            {
                {
                    "TOPIC_PHISHING", new string[]
                    {
                        "Phishing tip: Always check the sender domain carefully. Attackers use look-alike addresses.",
                        "Phishing tip: Hover over links before clicking to reveal the real destination URL.",
                        "Phishing tip: Legitimate companies never ask for your password via email."
                    }
                },
                {
                    "TOPIC_PASSWORD", new string[]
                    {
                        "Password tip: Use a unique 14 or more character password for every account.",
                        "Password tip: Passphrases like Elephant!Surf42Blue are both strong and memorable.",
                        "Password tip: Never share your passwords with anyone."
                    }
                },
                {
                    "TOPIC_2FA", new string[]
                    {
                        "2FA tip: Enable two-factor authentication on your email and banking accounts first.",
                        "2FA tip: Authenticator apps are significantly safer than SMS codes.",
                        "2FA tip: Even if your password is stolen, 2FA stops attackers from logging in."
                    }
                },
                {
                    "TOPIC_MALWARE", new string[]
                    {
                        "Malware tip: Keep your OS updated, most malware exploits known vulnerabilities.",
                        "Malware tip: Back up your data regularly to protect against ransomware.",
                        "Malware tip: Only download software from official and verified sources."
                    }
                },
                {
                    "TOPIC_VPN", new string[]
                    {
                        "VPN tip: Always use a VPN when connecting to public Wi-Fi networks.",
                        "VPN tip: Choose a provider with a verified no-logs policy."
                    }
                },
                {
                    "TOPIC_SCAM", new string[]
                    {
                        "Scam tip: If an offer sounds too good to be true, it almost certainly is.",
                        "Scam tip: Hang up immediately on unexpected calls claiming to be tech support."
                    }
                },
                {
                    "TOPIC_PRIVACY", new string[]
                    {
                        "Privacy tip: Review your app permissions monthly and revoke anything unnecessary.",
                        "Privacy tip: Use DuckDuckGo instead of Google to avoid search tracking."
                    }
                },
                {
                    "TOPIC_FIREWALL", new string[]
                    {
                        "Firewall tip: Always keep your firewall enabled, even on home networks.",
                        "Firewall tip: Regularly review your firewall rules to ensure nothing suspicious is allowed."
                    }
                }
            };
        }

        public string[] ProcessInput(string input, ObservableCollection<TaskItem> tasks)
        {
            string lower = input.ToLower().Trim();
            string intent = DetectIntent(lower);

            string response;

            if (intent == "ADD_TASK")
                response = HandleAddTask(input, lower, tasks);
            else if (intent == "SET_REMINDER")
                response = HandleSetReminder(tasks);
            else if (intent == "VIEW_TASKS")
                response = HandleViewTasks(tasks);
            else if (intent == "START_QUIZ")
                response = "Head to the Quiz tab to start! Test your cybersecurity knowledge.";
            else if (intent == "VIEW_LOG")
                response = HandleViewLog();
            else if (intent == "GREETING")
                response = "Hi " + UserName + "! Ask me anything about cybersecurity!";
            else if (intent == "THANKS")
                response = "You are welcome, " + UserName + "! Stay vigilant!";
            else if (intent == "HELP")
                response = BuildHelp();
            else if (intent.StartsWith("TOPIC_"))
                response = GetTip(intent);
            else
                response = HandleUnknown(lower);

            if (string.IsNullOrEmpty(intent))
                intent = "Unknown";

            return new string[] { response, intent };
        }

        private string HandleAddTask(string original, string lower, ObservableCollection<TaskItem> tasks)
        {
            string title = original;
            string[] removeWords = { "add a task to", "add task to", "create task to", "add a task", "create a task", "new task" };

            foreach (string w in removeWords)
                title = title.Replace(w, "", StringComparison.OrdinalIgnoreCase).Trim();

            if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
                title = "Cybersecurity review";

            string capitalized = char.ToUpper(title[0]) + title.Substring(1);

            var task = new TaskItem
            {
                Title = capitalized,
                Description = "Created via NLP: " + original,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            if (lower.Contains("remind") || lower.Contains("reminder"))
                task.ReminderDate = DateTime.Now.AddDays(1);

            tasks.Add(task);
            _log.Log("\uE8F9", "#23863633", "NLP task added", task.Title);

            if (task.ReminderDate.HasValue)
                return "Task added: " + task.Title + " with a reminder set for tomorrow. Check the Tasks tab!";
            else
                return "Task added: " + task.Title + ". You can view it in the Tasks tab. Would you like to set a reminder?";
        }

        private string HandleSetReminder(ObservableCollection<TaskItem> tasks)
        {
            TaskItem lastPending = null;

            foreach (var t in tasks)
            {
                if (t.Status != "Complete" && t.ReminderDate == null)
                    lastPending = t;
            }

            if (lastPending == null)
                return "You have no tasks without reminders. Add a task first in the Tasks tab!";

            lastPending.ReminderDate = DateTime.Now.AddDays(1);
            _log.Log("\uEC92", "#F7816633", "NLP reminder set", lastPending.Title);
            return "Reminder set for " + lastPending.Title + " tomorrow! You can adjust the date in the Tasks tab.";
        }

        private string HandleViewTasks(ObservableCollection<TaskItem> tasks)
        {
            if (tasks.Count == 0)
                return "You have no tasks yet. Go to the Tasks tab to add some!";

            List<TaskItem> pending = new List<TaskItem>();
            int doneCount = 0;

            foreach (var t in tasks)
            {
                if (t.Status != "Complete")
                    pending.Add(t);
                else
                    doneCount++;
            }

            string result = "You have " + tasks.Count + " task(s) total:\n";
            int shown = 0;

            foreach (var t in pending)
            {
                if (shown >= 5) break;
                result += "- " + t.Title;
                if (t.ReminderDate.HasValue)
                    result += " (reminder: " + t.ReminderDate.Value.ToString("dd MMM") + ")";
                result += "\n";
                shown++;
            }

            if (pending.Count > 5)
                result += "...and " + (pending.Count - 5) + " more in the Tasks tab.\n";

            if (doneCount > 0)
                result += doneCount + " task(s) completed.";

            return result;
        }

        private string HandleViewLog()
        {
            int count = 0;
            string result = "Here is a summary of recent actions:\n";
            int i = 1;

            foreach (var item in _log.LogItems)
            {
                if (count >= 5) break;
                result += i + ". " + item.Action + ": " + item.Detail + " (" + item.Timestamp + ")\n";
                i++;
                count++;
            }

            if (count == 0)
                return "No activity logged yet.";

            result += "\nSee the Activity Log tab for the full history.";
            return result;
        }

        private string GetTip(string intent)
        {
            if (_tips.ContainsKey(intent))
            {
                string[] t = _tips[intent];
                return t[_rng.Next(t.Length)];
            }
            return "Try asking something more specific about that topic!";
        }

        private string HandleUnknown(string lower)
        {
            string[] cyberWords = { "phish", "hack", "secur", "safe", "protect", "attack", "breach", "encrypt" };
            foreach (string w in cyberWords)
                if (lower.Contains(w))
                    return "That sounds like a cybersecurity question! Try asking about phishing, passwords, 2FA, malware, VPN, or scams.";

            return "I did not quite understand that. Here are some things you can say:\n" +
                   "- Tell me about phishing\n" +
                   "- Add a task to enable 2FA\n" +
                   "- Remind me to update my password\n" +
                   "- Show activity log\n" +
                   "- Start the quiz";
        }

        private string BuildHelp()
        {
            return "NLP Assistant things I understand:\n\n" +
                   "Tasks: Add a task to enable 2FA, Remind me to update my password\n" +
                   "Quiz: Start the quiz, Test my knowledge\n" +
                   "Tips: Tell me about phishing, How do I make strong passwords\n" +
                   "Log: Show activity log, What have you done for me\n\n" +
                   "I understand natural variations of all these phrases!";
        }

        private string DetectIntent(string lower)
        {
            foreach (var kvp in _intents)
                if (lower.Contains(kvp.Key))
                    return kvp.Value;
            return "";
        }
    }
}