using System.Data.SQLite;

namespace Powork.Repository
{
    public static class FileRepository
    {
        public static void InsertFile(string guid, string path)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"INSERT INTO TFile (id, path) VALUES ('{guid}', '{path}')";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static string SelectFile(string guid)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"SELECT * FROM TFile WHERE id = '{guid}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["path"].ToString();
                    }
                }
            }
            return null;
        }

        public static string RemoveFile(string guid)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"DELETE FROM TFile WHERE id = '{guid}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["path"].ToString();
                    }
                }
            }
            return null;
        }
    }
}
