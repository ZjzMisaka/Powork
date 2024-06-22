using System.Data.SQLite;
using Newtonsoft.Json;
using Powork.Model;

namespace Powork.Repository
{
    public class DelaySendingMessageRepository
    {
        public static void InsertMessage(string toIP, string message)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"INSERT INTO TDelaySendingMessage (toIP, message) VALUES (@toIP, @message)";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.Add(new SQLiteParameter("@toIP", toIP));
                command.Parameters.Add(new SQLiteParameter("@message", message));
                command.ExecuteNonQuery();
            }
        }

        public static List<string> SelectMessgae(string toIP)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            List<string> messageList = new List<string>();

            string sql = $"SELECT * FROM TTeamMessage WHERE toIP = '{toIP}' ORDER BY time ASC";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        messageList.Add(reader["message"].ToString());
                    }
                }
            }
            return messageList;
        }

        public static void RemoveMessgae(string toIP)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            List<string> messageList = new List<string>();

            string sql = $"DELETE FROM TTeamMessage WHERE toIP = '{toIP}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
