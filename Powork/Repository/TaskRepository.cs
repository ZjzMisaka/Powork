using Powork.Model;
using System.Data.SQLite;

namespace Powork.Repository
{
    public static class TaskRepository
    {
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
