using System;
using System.Collections.Generic;

namespace CyberBot.Services
{
    public class ChatbotEngine
    {
        public string UserName { get; set; } = "User";
        private readonly ActivityLogService _log;
        private readonly Random _rng = new();
        private readonly Dictionary<string, string> _memory = new();
        private string _lastTopic = "";

        private readonly Dictionary<string, List<string>> _responses = new()
        {
            ["phishing"] = new() {
                "Phishing attacks trick you into revealing info. Always verify the sender's email before clicking links.",
                "Spot phishing: urgency, misspelled domains, and password requests are red flags.",
                "Hover over links before clicking — if the URL looks odd, do not click!",
                "Legitimate companies will NEVER ask for your password via email."
            },
            ["password"] = new() {
                "Use at least 12 characters mixing uppercase, lowercase, numbers, and symbols.",
                "Use a password manager like Bitwarden to store unique passwords for every site.",
                "Never reuse passwords — a breach on one site can compromise all accounts.",
                "Passphrases like Coffee!River42Lamp are strong AND memorable!"
            },
            ["2fa"] = new() {
                "2FA adds a second layer of security — even stolen passwords will not let attackers in.",
                "Enable 2FA on email, banking, and social media first.",
                "Authenticator apps like Authy and Google Authenticator are safer than SMS codes."
            },
            ["malware"] = new() {
                "Keep your OS and antivirus updated — most malware exploits known vulnerabilities.",
                "Never download software from untrusted sources.",
                "Regular backups protect you from ransomware attacks."
            },
            ["vpn"] = new() {
                "A VPN encrypts your traffic, protecting you on public Wi-Fi.",
                "VPNs hide your IP and prevent ISP tracking.",
                "Choose a reputable VPN with a no-logs policy — avoid free VPNs."
            },
            ["privacy"] = new() {
                "Review app permissions regularly — does that flashlight app need your contacts?",
                "Use privacy-focused browsers like Firefox and Brave to reduce tracking.",
                "Use uBlock Origin to block trackers and clear cookies regularly."
            },
            ["scam"] = new() {
                "Common scams: fake tech support, lottery wins, urgent wire transfers.",
                "Hang up on unexpected calls asking for remote access to your PC!",
                "Verify by looking up the company's official number and calling them directly."
            },
            ["wifi"] = new() {
                "Never access banking on public Wi-Fi without a VPN.",
                "Secure your home Wi-Fi with WPA3 encryption and a strong password.",
                "Disable Wi-Fi auto-connect to prevent joining rogue hotspots."
            },
            ["update"] = new() {
                "Enable automatic updates — patches fix critical security vulnerabilities.",
                "Unpatched software is a top attack vector. Do not delay updates!",
                "This includes your OS, browser, antivirus, and all apps."
            },
            ["ransomware"] = new() {
                "Ransomware encrypts your files and demands payment. Always keep backups!",
                "Never pay the ransom — there is no guarantee you will get your files back.",
                "Disconnect from the network immediately if you suspect a ransomware infection."
            },
            ["firewall"] = new() {
                "A firewall monitors incoming and outgoing traffic to block unauthorised access.",
                "Keep your firewall enabled at all times, even on home networks.",
                "Both hardware and software firewalls add important layers of protection."
            },
            ["hello"] = new() {
                "Hey {name}! How can I help you stay secure today?",
                "Hi {name}! Ready to talk cybersecurity?"
            },
            ["hi"] = new() {
                "Hi {name}! What cybersecurity topic can I help with?",
                "Hello {name}! Stay safe online!"
            },
            ["help"] = new() {
                "I can help with:\n- Phishing\n- Passwords\n- 2FA\n- Malware\n- VPN\n- Scams\n- Privacy\n- Wi-Fi\n- Updates\n- Ransomware\n- Firewalls\n\nJust type any topic!"
            },
            ["thanks"] = new() {
                "You are welcome, {name}! Stay safe!",
                "Happy to help! Stay vigilant!"
            },
            ["bye"] = new() {
                "Goodbye, {name}! Stay cyber safe!",
                "See you later, {name}! Keep your software updated!"
            }
        };

        private readonly Dictionary<string, string> _sentimentMap = new()
        {
            ["worried"] = "concerned",
            ["scared"] = "concerned",
            ["anxious"] = "concerned",
            ["frustrated"] = "frustrated",
            ["confused"] = "confused",
            ["angry"] = "frustrated",
            ["happy"] = "positive",
            ["curious"] = "curious"
        };

        private readonly Dictionary<string, string> _sentimentPrefixes = new()
        {
            ["concerned"] = "I understand you are concerned — that is completely valid. Let me help! ",
            ["frustrated"] = "I hear you — let us tackle this step by step. ",
            ["confused"] = "No worries, let us break it down together. "
        };

        public ChatbotEngine(ActivityLogService log)
        {
            _log = log;
        }

        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please type something so I can help!";

            string lower = input.ToLower().Trim();

            // Memory: store name
            if (lower.Contains("my name is"))
            {
                int idx = lower.IndexOf("my name is") + "my name is".Length;
                string name = input.Substring(idx).Trim().Split(' ')[0];
                _memory["name"] = name;
                UserName = name;
                return $"Nice to meet you, {name}! How can I help you stay secure?";
            }

            // Memory recall
            if (lower.Contains("what is my name") || lower.Contains("do you know my name"))
                return _memory.ContainsKey("name")
                    ? $"Your name is {_memory["name"]}!"
                    : "I do not know your name yet. Tell me by saying 'My name is ...'";

            // Conversation flow: follow-up
            if ((lower.Contains("tell me more") || lower.Contains("explain more") ||
                 lower.Contains("another tip") || lower.Contains("give me a tip"))
                && !string.IsNullOrEmpty(_lastTopic))
                return Pick(_lastTopic).Replace("{name}", UserName);

            // Sentiment detection
            string sentiment = DetectSentiment(lower);
            string prefix = sentiment != "" && _sentimentPrefixes.ContainsKey(sentiment)
                ? _sentimentPrefixes[sentiment] : "";

            // Keyword matching
            foreach (var kvp in _responses)
            {
                if (lower.Contains(kvp.Key))
                {
                    _lastTopic = kvp.Key;
                    return prefix + Pick(kvp.Key).Replace("{name}", UserName);
                }
            }

            // Tab redirects
            if (lower.Contains("task") || lower.Contains("reminder"))
                return prefix + "You can manage your tasks in the Tasks tab!";
            if (lower.Contains("quiz") || lower.Contains("game") || lower.Contains("test"))
                return prefix + "Head to the Quiz tab to test your cybersecurity knowledge!";
            if (lower.Contains("log") || lower.Contains("activity") || lower.Contains("history"))
                return "Check the Activity Log tab to see everything that has been tracked!";
            if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey"))
                return $"Hello, {UserName}! Ask me about phishing, passwords, 2FA, malware, or VPNs!";

            // Default error handling
            return "I did not quite understand that. Try topics like phishing, passwords, 2FA, malware, or VPN. Type 'help' for all options!";
        }

        private string Pick(string key)
        {
            var list = _responses[key];
            return list[_rng.Next(list.Count)];
        }

        private string DetectSentiment(string input)
        {
            foreach (var kvp in _sentimentMap)
                if (input.Contains(kvp.Key)) return kvp.Value;
            return "";
        }
    }
}
