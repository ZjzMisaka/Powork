using Powork.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using Task = Powork.Model.Task;

namespace Powork.Repository
{
    public static class TaskRepository
    {
        public static List<Task> SelectTasksByMonth(string projectId, int year, int month)
        {
            var tasks = new List<Task>();
            if (string.IsNullOrEmpty(projectId)) return tasks;

            SQLiteConnection connection = CommonRepository.GetConnection();
            string sql = $"SELECT * FROM TTask WHERE projectId = '{projectId}' AND year = {year} AND month = {month}";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tasks.Add(new Task
                    {
                        Id = reader["id"].ToString(),
                        ProjectId = reader["projectId"].ToString(),
                        Name = reader["name"].ToString(),
                        Year = System.Convert.ToInt32(reader["year"]),
                        Month = System.Convert.ToInt32(reader["month"]),
                        StartDay = System.Convert.ToInt32(reader["startDay"]),
                        Days = System.Convert.ToInt32(reader["days"]),
                        Note = reader["note"].ToString()
                    });
                }
            }
            return tasks;
        }

        public static List<Task> SelectTasksById(string id)
        {
            var tasks = new List<Task>();
            SQLiteConnection connection = CommonRepository.GetConnection();
            string sql = $"SELECT * FROM TTask WHERE id = '{id}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    tasks.Add(new Task
                    {
                        Id = reader["id"].ToString(),
                        ProjectId = reader["projectId"].ToString(),
                        Name = reader["name"].ToString(),
                        Year = System.Convert.ToInt32(reader["year"]),
                        Month = System.Convert.ToInt32(reader["month"]),
                        StartDay = System.Convert.ToInt32(reader["startDay"]),
                        Days = System.Convert.ToInt32(reader["days"]),
                        Note = reader["note"].ToString()
                    });
                }
            }
            return tasks;
        }

        public static void InsertOrUpdateTask(Task task)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"INSERT OR REPLACE INTO TTask (id, projectId, name, year, month, startDay, days, note) VALUES ('{task.Id}', '{task.ProjectId}', '{task.Name}', {task.Year}, {task.Month}, {task.StartDay}, {task.Days}, '{task.Note}')";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveTask(string id)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"DELETE FROM TTask WHERE id = '{id}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
