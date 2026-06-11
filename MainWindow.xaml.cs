using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CyberBot.Models;
using CyberBot.Services;

namespace CyberBot
{
    public partial class MainWindow : Window
    {
        private readonly ChatbotEngine _chatbot;
        private readonly QuizService _quizService;
        private readonly NlpService _nlpService;
        private readonly ActivityLogService _activityLog;
        private readonly DatabaseService _database;

        private readonly ObservableCollection<TaskItem> _tasks = new ObservableCollection<TaskItem>();

        private int _quizScore = 0;
        private int _currentQuestionIndex = 0;
        private List<QuizQuestion> _currentQuiz = new List<QuizQuestion>();
        private bool _answerSelected = false;

        private int _chatCount = 0;
        private int _quizCount = 0;
        private int _nlpCount = 0;
        private bool _showAllLog = false;

        public MainWindow()
        {
            InitializeComponent();

            _activityLog = new ActivityLogService();
            _database = new DatabaseService();
            _chatbot = new ChatbotEngine(_activityLog);
            _quizService = new QuizService();
            _nlpService = new NlpService(_activityLog);

            lstTasks.ItemsSource = _tasks;

            LoadTasksFromDatabase();
            AskUserName();

            AddBotMessage(ChatPanel, "Welcome to CyberBot! Type anything to get started, or type help to see what I can do.");
            _activityLog.Log("\uE946", "#1F6FEB33", "Session started", "CyberBot launched");
            RefreshStats();
            RefreshLogDisplay();
        }

        private void AskUserName()
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(
                "Welcome to CyberBot!\nPlease enter your name:", "Welcome", "User");
            if (!string.IsNullOrWhiteSpace(name))
            {
                _chatbot.UserName = name;
                _nlpService.UserName = name;
                txtUserName.Text = name;
            }

            PlayGreeting(name);
        }

        private void PlayGreeting(string name)
        {
            try
            {
                SpeechSynthesizer synth = new SpeechSynthesizer();
                synth.Rate = -1;
                synth.Volume = 100;
                string greeting = "Welcome to CyberBot, " + name + ". Your personal cybersecurity awareness assistant. I am here to help you stay safe online. Let us get started!";
                synth.SpeakAsync(greeting);
            }
            catch
            {
                // Continue silently if speech not available
            }
        }

        // ── CHAT ──────────────────────────────────────────────────────
        private void TxtChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnSend_Click(sender, e);
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            string input = txtChatInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            AddUserMessage(ChatPanel, input);
            txtChatInput.Clear();

            string response = _chatbot.GetResponse(input);
            AddBotMessage(ChatPanel, response);

            _chatCount++;
            statChat.Text = _chatCount.ToString();
            string logDetail = input.Length > 40 ? input.Substring(0, 40) + "..." : input;
            _activityLog.Log("\uE8BD", "#1F6FEB33", "Chat message", logDetail);
            RefreshLogDisplay();
            ChatScrollViewer.ScrollToBottom();
        }

        private void BtnClearChat_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            AddBotMessage(ChatPanel, "Chat cleared! How can I help you?");
        }

        // ── TASKS ─────────────────────────────────────────────────────
        private void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTaskTitle.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var task = new TaskItem
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Description = txtTaskDesc.Text.Trim(),
                ReminderDate = dpReminder.SelectedDate,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _tasks.Add(task);
            _database.SaveTask(task);
            _activityLog.Log("\uE8F9", "#23863633", "Task added", title);
            RefreshStats();
            RefreshLogDisplay();

            txtTaskTitle.Clear();
            txtTaskDesc.Clear();
            dpReminder.SelectedDate = null;

            MessageBox.Show("Task " + title + " added successfully!", "Task Added",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnQuickTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                txtTaskTitle.Text = btn.Tag != null ? btn.Tag.ToString() : "";
        }

        private void BtnMarkComplete_Click(object sender, RoutedEventArgs e)
        {
            if (lstTasks.SelectedItem is TaskItem task)
            {
                task.Status = "Complete";
                _database.UpdateTaskStatus(task.Id, "Complete");
                _activityLog.Log("\uE930", "#23863633", "Task completed", task.Title);
                RefreshStats();
                RefreshLogDisplay();
                lstTasks.Items.Refresh();
            }
            else
                MessageBox.Show("Please select a task first.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (lstTasks.SelectedItem is TaskItem task)
            {
                MessageBoxResult result = MessageBox.Show("Delete " + task.Title + "?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _database.DeleteTask(task.Id);
                    _tasks.Remove(task);
                    _activityLog.Log("\uE74D", "#DA363333", "Task deleted", task.Title);
                    RefreshStats();
                    RefreshLogDisplay();
                }
            }
            else
                MessageBox.Show("Please select a task first.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadTasksFromDatabase()
        {
            List<TaskItem> loaded = _database.GetAllTasks();
            foreach (var t in loaded)
                _tasks.Add(t);
        }

        // ── QUIZ ──────────────────────────────────────────────────────
        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            _currentQuiz = _quizService.GetRandomQuiz();
            _quizScore = 0;
            _currentQuestionIndex = 0;
            _quizCount++;
            statQuiz.Text = _quizCount.ToString();

            QuizStartPanel.Visibility = Visibility.Collapsed;
            QuizResultPanel.Visibility = Visibility.Collapsed;
            QuizPlayPanel.Visibility = Visibility.Visible;

            _activityLog.Log("\uE7FC", "#F7816633", "Quiz started", "New quiz session");
            RefreshLogDisplay();
            LoadQuestion();
        }

        private void LoadQuestion()
        {
            if (_currentQuestionIndex >= _currentQuiz.Count)
            {
                ShowResults();
                return;
            }

            var q = _currentQuiz[_currentQuestionIndex];
            _answerSelected = false;
            FeedbackBorder.Visibility = Visibility.Collapsed;
            AnswersPanel.Children.Clear();

            txtQuizProgress.Text = "Question " + (_currentQuestionIndex + 1) + " of " + _currentQuiz.Count;
            QuizProgress.Value = _currentQuestionIndex + 1;
            QuizProgress.Maximum = _currentQuiz.Count;
            txtQuizScore.Text = "Score: " + _quizScore;
            txtQuestion.Text = q.Question;
            txtQuizType.Text = q.IsTrueFalse ? "TRUE / FALSE" : "MULTIPLE CHOICE";

            List<string> options;
            if (q.IsTrueFalse)
                options = new List<string> { "True", "False" };
            else
                options = q.Options;

            char letter = 'A';
            foreach (var opt in options)
            {
                var btn = new Button
                {
                    Content = "  " + letter + ")  " + opt,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
                    Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(16, 12, 16, 12),
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 10),
                    Cursor = Cursors.Hand,
                    Tag = opt
                };
                btn.Click += AnswerButton_Click;
                AnswersPanel.Children.Add(btn);
                letter++;
            }
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_answerSelected) return;
            _answerSelected = true;

            var btn = (Button)sender;
            string selected = btn.Tag != null ? btn.Tag.ToString() : "";
            var q = _currentQuiz[_currentQuestionIndex];
            bool correct = selected.Equals(q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

            foreach (Button b in AnswersPanel.Children)
            {
                string bText = b.Tag != null ? b.Tag.ToString() : "";
                if (bText.Equals(q.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                    b.Background = new SolidColorBrush(Color.FromRgb(35, 134, 54));
                else if (b == btn && !correct)
                    b.Background = new SolidColorBrush(Color.FromRgb(218, 54, 51));
                b.IsEnabled = false;
            }

            if (correct) _quizScore++;
            txtQuizScore.Text = "Score: " + _quizScore;

            FeedbackBorder.Visibility = Visibility.Visible;
            if (correct)
                FeedbackBorder.Background = new SolidColorBrush(Color.FromArgb(40, 35, 134, 54));
            else
                FeedbackBorder.Background = new SolidColorBrush(Color.FromArgb(40, 218, 54, 51));

            txtFeedback.Text = correct ? "Correct!" : "Incorrect! The answer is: " + q.CorrectAnswer;
            txtFeedbackExplanation.Text = q.Explanation;

            string logDetail = q.Question.Length > 50 ? q.Question.Substring(0, 50) + "..." : q.Question;
            _activityLog.Log("\uE7C3", "#1F6FEB33", correct ? "Quiz correct" : "Quiz incorrect", logDetail);
            RefreshLogDisplay();
        }

        private void BtnNextQuestion_Click(object sender, RoutedEventArgs e)
        {
            _currentQuestionIndex++;
            LoadQuestion();
        }

        private void ShowResults()
        {
            QuizPlayPanel.Visibility = Visibility.Collapsed;
            QuizResultPanel.Visibility = Visibility.Visible;

            int total = _currentQuiz.Count;
            double pct = (double)_quizScore / total * 100;
            txtResultScore.Text = _quizScore + " / " + total;

            if (pct >= 80)
            {
                txtResultIcon.Text = "\uE734";
                txtResultTitle.Text = "Excellent!";
                txtResultFeedback.Text = "Great job! You are a cybersecurity pro!";
                txtResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0));
            }
            else if (pct >= 60)
            {
                txtResultIcon.Text = "\uE8DC";
                txtResultTitle.Text = "Good Work!";
                txtResultFeedback.Text = "You know your stuff! Keep learning to sharpen your skills.";
                txtResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(63, 185, 80));
            }
            else
            {
                txtResultIcon.Text = "\uE82D";
                txtResultTitle.Text = "Keep Learning!";
                txtResultFeedback.Text = "Review the topics and try again!";
                txtResultTitle.Foreground = new SolidColorBrush(Color.FromRgb(247, 129, 102));
            }

            string logText = "Score: " + _quizScore + "/" + total + " (" + pct.ToString("0") + "%)";
            _activityLog.Log("\uE930", "#F7816633", "Quiz completed", logText);
            RefreshLogDisplay();
        }

        private void BtnPlayAgain_Click(object sender, RoutedEventArgs e)
        {
            QuizResultPanel.Visibility = Visibility.Collapsed;
            QuizStartPanel.Visibility = Visibility.Visible;
        }

        // ── NLP ───────────────────────────────────────────────────────
        private void TxtNlpInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BtnNlpSend_Click(sender, e);
        }

        private void BtnNlpSend_Click(object sender, RoutedEventArgs e)
        {
            string input = txtNlpInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            AddUserMessage(NlpChatPanel, input);
            txtNlpInput.Clear();

            string[] result = _nlpService.ProcessInput(input, _tasks);
            string response = result[0];
            string intent = result[1];

            AddBotMessage(NlpChatPanel, response);
            txtDetectedIntent.Text = intent;

            _nlpCount++;
            statNlp.Text = _nlpCount.ToString();
            _activityLog.Log("\uE9CE", "#D2A8FF33", "NLP interaction", "Intent: " + intent);
            RefreshLogDisplay();
            NlpScrollViewer.ScrollToBottom();
        }

        // ── ACTIVITY LOG ──────────────────────────────────────────────
        private void BtnShowAllLog_Click(object sender, RoutedEventArgs e)
        {
            _showAllLog = !_showAllLog;
            btnToggleLog.Content = _showAllLog ? "Show Recent" : "Show All";
            RefreshLogDisplay();
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Clear all log entries?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _activityLog.Clear();
                RefreshLogDisplay();
            }
        }

        private void RefreshLogDisplay()
        {
            List<ActivityLogItem> items = new List<ActivityLogItem>();

            if (_showAllLog)
            {
                foreach (var item in _activityLog.LogItems)
                    items.Add(item);
            }
            else
            {
                int count = 0;
                foreach (var item in _activityLog.LogItems)
                {
                    if (count >= 10) break;
                    items.Add(item);
                    count++;
                }
            }

            lstActivityLog.ItemsSource = null;
            lstActivityLog.ItemsSource = items;

            int shown = _showAllLog ? _activityLog.LogItems.Count : Math.Min(10, _activityLog.LogItems.Count);
            if (_showAllLog)
                txtLogCount.Text = "Showing all " + _activityLog.LogItems.Count + " entries";
            else
                txtLogCount.Text = "Showing last " + shown + " of " + _activityLog.LogItems.Count + " actions";
        }

        private void RefreshStats()
        {
            int activeTasks = 0;
            foreach (var t in _tasks)
                if (t.Status != "Complete") activeTasks++;

            statTasks.Text = activeTasks.ToString();
            statQuiz.Text = _quizCount.ToString();
            statChat.Text = _chatCount.ToString();
            statNlp.Text = _nlpCount.ToString();
        }

        // ── UI HELPERS ────────────────────────────────────────────────
        private void AddUserMessage(StackPanel panel, string text)
        {
            var container = new Grid();
            container.Margin = new Thickness(0, 0, 0, 10);

            var bubble = new Border();
            bubble.Background = new SolidColorBrush(Color.FromRgb(31, 111, 235));
            bubble.CornerRadius = new CornerRadius(12, 12, 4, 12);
            bubble.Padding = new Thickness(14, 10, 14, 10);
            bubble.MaxWidth = 500;
            bubble.HorizontalAlignment = HorizontalAlignment.Right;

            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = Brushes.White;
            textBlock.FontSize = 13;
            textBlock.TextWrapping = TextWrapping.Wrap;

            bubble.Child = textBlock;
            container.Children.Add(bubble);
            panel.Children.Add(container);
        }

        private void AddBotMessage(StackPanel panel, string text)
        {
            var row = new StackPanel();
            row.Orientation = Orientation.Horizontal;
            row.Margin = new Thickness(0, 0, 0, 10);

            var avatar = new Border();
            avatar.Width = 34;
            avatar.Height = 34;
            avatar.Background = new SolidColorBrush(Color.FromRgb(31, 111, 235));
            avatar.CornerRadius = new CornerRadius(17);
            avatar.Margin = new Thickness(0, 0, 10, 0);
            avatar.VerticalAlignment = VerticalAlignment.Top;

            var avatarIcon = new TextBlock();
            avatarIcon.Text = "\uE83D";
            avatarIcon.FontFamily = new FontFamily("Segoe MDL2 Assets");
            avatarIcon.FontSize = 16;
            avatarIcon.Foreground = Brushes.White;
            avatarIcon.HorizontalAlignment = HorizontalAlignment.Center;
            avatarIcon.VerticalAlignment = VerticalAlignment.Center;
            avatar.Child = avatarIcon;

            var bubble = new Border();
            bubble.Background = new SolidColorBrush(Color.FromRgb(22, 27, 34));
            bubble.BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61));
            bubble.BorderThickness = new Thickness(1);
            bubble.CornerRadius = new CornerRadius(4, 12, 12, 12);
            bubble.Padding = new Thickness(14, 10, 14, 10);
            bubble.MaxWidth = 560;

            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(201, 209, 217));
            textBlock.FontSize = 13;
            textBlock.TextWrapping = TextWrapping.Wrap;
            bubble.Child = textBlock;

            row.Children.Add(avatar);
            row.Children.Add(bubble);
            panel.Children.Add(row);
        }
    }
}