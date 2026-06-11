using System;
using System.Collections.Generic;
using System.Linq;
using CyberBot.Models;

namespace CyberBot.Services
{
    public class QuizService
    {
        private readonly Random _rng = new();

        private readonly List<QuizQuestion> _all = new()
        {
            new() {
                Question = "What should you do if you receive an email asking for your password?",
                Options = new() { "Reply with your password", "Delete the email", "Report as phishing", "Ignore it" },
                CorrectAnswer = "Report as phishing",
                Explanation = "Legitimate companies never ask for passwords via email. Reporting phishing helps protect others too."
            },
            new() {
                Question = "What is the minimum recommended length for a strong password?",
                Options = new() { "6 characters", "8 characters", "12 characters", "16 characters" },
                CorrectAnswer = "12 characters",
                Explanation = "12 or more characters significantly increases resistance to brute-force attacks."
            },
            new() {
                Question = "Which form of two-factor authentication is most secure?",
                Options = new() { "SMS text message", "Email code", "Authenticator app", "Security question" },
                CorrectAnswer = "Authenticator app",
                Explanation = "Authenticator apps generate time-based codes that cannot be intercepted via SIM swapping unlike SMS."
            },
            new() {
                Question = "What does HTTPS mean for a website?",
                Options = new() { "The site is trustworthy", "The connection is encrypted", "No viruses are present", "Your data is never stored" },
                CorrectAnswer = "The connection is encrypted",
                Explanation = "HTTPS encrypts data in transit but does not mean the site itself is safe or legitimate."
            },
            new() {
                Question = "What is a zero-day vulnerability?",
                Options = new() { "A virus that activates at midnight", "A flaw with no patch yet", "Software expiring soon", "A free security tool" },
                CorrectAnswer = "A flaw with no patch yet",
                Explanation = "Zero-day flaws are dangerous because attackers exploit them before developers can release a fix."
            },
            new() {
                Question = "Which attack tricks people into giving up information through deception?",
                Options = new() { "SQL Injection", "Social Engineering", "Brute Force", "Man-in-the-Middle" },
                CorrectAnswer = "Social Engineering",
                Explanation = "Social engineering exploits human psychology rather than technical vulnerabilities."
            },
            new() {
                Question = "What is ransomware?",
                Options = new() { "Software that speeds up your PC", "Malware that encrypts files and demands payment", "A type of VPN", "A phishing website" },
                CorrectAnswer = "Malware that encrypts files and demands payment",
                Explanation = "Ransomware encrypts your files and demands cryptocurrency payment for the decryption key."
            },
            new() {
                Question = "What is the main purpose of a VPN?",
                Options = new() { "Speed up internet", "Block all viruses", "Encrypt traffic and hide your IP", "Increase storage" },
                CorrectAnswer = "Encrypt traffic and hide your IP",
                Explanation = "VPNs create encrypted tunnels masking your IP address — vital when using public Wi-Fi."
            },
            new() {
                Question = "Which Wi-Fi security protocol is currently the strongest?",
                Options = new() { "WEP", "WPA", "WPA2", "WPA3" },
                CorrectAnswer = "WPA3",
                Explanation = "WPA3 is the latest standard offering improved encryption and brute-force attack protection."
            },
            new() {
                Question = "What is shoulder surfing?",
                Options = new() { "Hacking via satellite", "Watching someone's screen to steal info", "A type of phishing email", "Installing spyware remotely" },
                CorrectAnswer = "Watching someone's screen to steal info",
                Explanation = "Always shield your screen when entering passwords or PINs in public places."
            },
            new() {
                IsTrueFalse = true,
                Question = "Using the same strong password on multiple websites is safe.",
                CorrectAnswer = "False",
                Explanation = "Password reuse enables credential stuffing — one breach can compromise all your accounts."
            },
            new() {
                IsTrueFalse = true,
                Question = "Enabling automatic software updates is a good cybersecurity practice.",
                CorrectAnswer = "True",
                Explanation = "Auto-updates deliver security patches quickly, closing vulnerabilities before attackers exploit them."
            },
            new() {
                IsTrueFalse = true,
                Question = "A padlock icon in your browser means a website is completely safe and legitimate.",
                CorrectAnswer = "False",
                Explanation = "The padlock only means the connection is encrypted. Phishing sites can also use HTTPS."
            },
            new() {
                IsTrueFalse = true,
                Question = "Public Wi-Fi is safe to use for online banking without any extra protection.",
                CorrectAnswer = "False",
                Explanation = "Public Wi-Fi enables man-in-the-middle attacks. Always use a VPN on public networks."
            },
            new() {
                IsTrueFalse = true,
                Question = "A firewall can help block unauthorised access to your computer or network.",
                CorrectAnswer = "True",
                Explanation = "Firewalls monitor traffic based on security rules, providing a barrier against unauthorised access."
            }
        };

        public List<QuizQuestion> GetRandomQuiz(int count = 10)
            => _all.OrderBy(_ => _rng.Next()).Take(count).ToList();
    }
}