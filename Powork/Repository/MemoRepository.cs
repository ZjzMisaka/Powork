using System.Data.SQLite;

namespace Powork.Repository
{
    public static class MemoRepository
    {
        public static string SelectMemo(string date)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT * FROM TMemo WHERE date = '{date}'";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["memo"].ToString();
                        }
                    }
                }
            }
            return "";
        }

        public static void InsertOrUpdateMemo(string date, string memo)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = @"INSERT OR REPLACE INTO TMemo (date, memo) VALUES (@date, @memo)";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@date", date);
                    command.Parameters.AddWithValue("@memo", memo);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
