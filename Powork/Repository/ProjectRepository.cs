using Powork.Model;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Powork.Repository
{
    public static class ProjectRepository
    {
        public static List<Project> SelectProjects()
        {
            var projects = new List<Project>();
            SQLiteConnection connection = CommonRepository.GetConnection();
            string sql = "SELECT * FROM TProject";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    projects.Add(new Project
                    {
                        Id = reader["id"].ToString(),
                        Name = reader["name"].ToString(),
                        Managers = reader["managers"].ToString()
                    });
                }
            }
            return projects;
        }

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
