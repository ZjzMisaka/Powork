using Powork.Model;
using System.Data.SQLite;

namespace Powork.Repository
{
    public static class ProjectRepository
    {
        public static void InsertOrUpdateProject(Project project)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"INSERT OR REPLACE INTO TProject (id, name, managers) VALUES ('{project.Id}', '{project.Name}', '{project.Managers}')";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveProject(string id)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"DELETE FROM TProject WHERE id = '{id}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
