using System.Data.SQLite;

namespace Powork.Repository
{
    public static class SettingRepository
    {
        public static string SelectSetting(string key)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"SELECT * FROM TSetting WHERE key = '{key}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["value"].ToString();
                    }
                }
            }
            return string.Empty;
        }

        public static void InsertSetting(string key, string value)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = @"INSERT INTO TSetting (key, value) VALUES (@key, @value)";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@value", value);
                command.ExecuteNonQuery();
            }
        }

        public static void UpdateSetting(string key, string value)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = @"UPDATE TSetting SET value = @value WHERE key = @key";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@value", value);
                command.ExecuteNonQuery();
            }
        }

        public static void SetDefault(string key, string value)
        {
            if (SelectSetting(key) == string.Empty)
            {
                InsertSetting(key, value);
            }
        }
    }
}
