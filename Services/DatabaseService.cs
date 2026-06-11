using System;
using System.Collections.Generic;
using CyberBot.Models;

// To start MySQL:
// 1. Right-click project → Manage NuGet Packages → search MySql.Data → Install
// 2. Uncomment the #define line below
// 3. Update the connection string with your MySQL password

//#define HAS_MYSQL

namespace CyberBot.Services
{
    public class DatabaseService
    {
        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=cyberbot_db;Uid=root;Pwd=YourPassword;";

        private bool _dbAvailable = false;

        public DatabaseService() { TestConnection(); }

        private void TestConnection()
        {
            try
            {
#if HAS_MYSQL
                using var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
                conn.Open();
                _dbAvailable = true;
#endif
            }
            catch { _dbAvailable = false; }
        }

        public void SaveTask(TaskItem task)
        {
            if (!_dbAvailable) return;
            try
            {
#if HAS_MYSQL
                using var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                    "INSERT INTO tasks (id,title,description,reminder,status,created_at) " +
                    "VALUES (@id,@title,@desc,@reminder,@status,@created)", conn);
                cmd.Parameters.AddWithValue("@id", task.Id);
                cmd.Parameters.AddWithValue("@title", task.Title);
                cmd.Parameters.AddWithValue("@desc", task.Description ?? "");
                cmd.Parameters.AddWithValue("@reminder", (object?)task.ReminderDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@status", task.Status);
                cmd.Parameters.AddWithValue("@created", task.CreatedAt);
                cmd.ExecuteNonQuery();
#endif
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        public void UpdateTaskStatus(string id, string status)
        {
            if (!_dbAvailable) return;
            try
            {
#if HAS_MYSQL
                using var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                    "UPDATE tasks SET status=@status WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
#endif
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        public void DeleteTask(string id)
        {
            if (!_dbAvailable) return;
            try
            {
#if HAS_MYSQL
                using var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                    "DELETE FROM tasks WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
#endif
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        public List<TaskItem> GetAllTasks()
        {
            var list = new List<TaskItem>();
            if (!_dbAvailable) return list;
            try
            {
#if HAS_MYSQL
                using var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySql.Data.MySqlClient.MySqlCommand(
                    "SELECT id,title,description,reminder,status,created_at " +
                    "FROM tasks ORDER BY created_at DESC", conn);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add(new TaskItem
                    {
                        Id = r.GetString(0),
                        Title = r.GetString(1),
                        Description = r.IsDBNull(2) ? "" : r.GetString(2),
                        ReminderDate = r.IsDBNull(3) ? null : r.GetDateTime(3),
                        Status = r.GetString(4),
                        CreatedAt = r.GetDateTime(5)
                    });
#endif
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
            return list;
        }
    }
}