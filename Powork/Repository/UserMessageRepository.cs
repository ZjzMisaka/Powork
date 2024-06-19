using System.Data.SQLite;
using Newtonsoft.Json;
using Powork.Model;

namespace Powork.Repository
{
    public class UserMessageRepository
    {
        public static void InsertMessage(UserMessage userMessage, string toIP, string toName)
        {
            string body = JsonConvert.SerializeObject(userMessage.MessageBody);
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO TUserMessage (body, type, time, fromIP, fromName, toIP, toName) VALUES (@body, @type, @time, @fromIP, @fromName, @toIP, @toName)";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.Add(new SQLiteParameter("@body", body));
                    command.Parameters.Add(new SQLiteParameter("@type", userMessage.Type));
                    command.Parameters.Add(new SQLiteParameter("@time", userMessage.Time));
                    command.Parameters.Add(new SQLiteParameter("@fromIP", userMessage.SenderIP));
                    command.Parameters.Add(new SQLiteParameter("@fromName", userMessage.SenderName));
                    command.Parameters.Add(new SQLiteParameter("@toIP", toIP));
                    command.Parameters.Add(new SQLiteParameter("@toName", toName));
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<UserMessage> SelectMessgae(string ip, string name, int id = -1)
        {
            List<UserMessage> userMessageList = new List<UserMessage>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT * FROM TUserMessage WHERE (fromIP='{ip}' AND fromName='{name}') OR (toIP='{ip}' AND toName='{name}')";
                if (id != -1)
                {
                    sql = $"{sql}  AND  id < {id}";
                }
                sql = $"{sql}  ORDER BY time DESC LIMIT 10";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userMessageList.Add(new UserMessage()
                            {
                                SenderIP = reader["fromIP"].ToString(),
                                SenderName = reader["fromName"].ToString(),
                                MessageBody = JsonConvert.DeserializeObject<List<TCPMessageBody>>(reader["body"].ToString()),
                                Type = (MessageType)(int.Parse(reader["type"].ToString())),
                                Time = reader["time"].ToString(),
                                ID = int.Parse(reader["id"].ToString()),
                            });
                        }
                        userMessageList.Reverse();
                    }
                }
            }
            return userMessageList;
        }
    }
}
