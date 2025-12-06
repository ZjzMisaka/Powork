using Powork.Model;
using System.Data.SQLite;

namespace Powork.Repository
{
    public static class ProgressRepository
    {
        public static Progress SelectProgress(string taskId, string projectId)
        {
            if (string.IsNullOrEmpty(taskId) || string.IsNullOrEmpty(projectId)) return null;

            SQLiteConnection connection = CommonRepository.GetConnection();
            string sql = $"SELECT * FROM TProgress WHERE taskId = '{taskId}' AND projectId = '{projectId}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new Progress
                    {
                        TaskId = reader["taskId"].ToString(),
                        ProjectId = reader["projectId"].ToString(),
                        Percentage = System.Convert.ToInt32(reader["percentage"])
                    };
                }
            }
            return null;
        }

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