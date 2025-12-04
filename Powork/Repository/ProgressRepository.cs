using Powork.Model;
using System.Data.SQLite;

namespace Powork.Repository
{
    public static class ProgressRepository
    {
        public static void InsertOrUpdateProgress(Progress progress)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"INSERT OR REPLACE INTO TProgress (taskId, projectId, percentage) VALUES ('{progress.TaskId}', '{progress.ProjectId}', {progress.Percentage})";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveProgress(string taskId)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"DELETE FROM TProgress WHERE taskId = '{taskId}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
