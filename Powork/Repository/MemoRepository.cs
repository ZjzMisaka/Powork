using System.Data.SQLite;

namespace Powork.Repository
{
    public static class MemoRepository
    {
        public static string SelectMemo(string date)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

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
            return string.Empty;
        }

        public static string SelectPreviousMemoDate(string date)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"SELECT * FROM TMemo WHERE date < '{date}' AND memo IS NOT NULL AND memo <> '' ORDER BY date DESC LIMIT 1";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["date"].ToString().Split(" ")[0];
                    }
                }
            }
            return null;
        }

        public static string SelectNextMemoDate(string date)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"SELECT * FROM TMemo WHERE date > '{date}' AND memo IS NOT NULL AND memo <> '' ORDER BY date ASC LIMIT 1";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["date"].ToString().Split(" ")[0];
                    }
                }
            }
            return null;
        }

        public static void InsertOrUpdateMemo(string date, string memo)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = @"INSERT OR REPLACE INTO TMemo (date, memo) VALUES (@date, @memo)";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@date", date);
                command.Parameters.AddWithValue("@memo", memo);
                command.ExecuteNonQuery();
            }
        }
    }
}
