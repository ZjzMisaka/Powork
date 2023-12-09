using Newtonsoft.Json;
using Powork.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Powork.Repository
{
    internal class UserMessageRepository
    {
        static public void InsertMessage(UserMessage userMessage)
        {
            string body = JsonConvert.SerializeObject(userMessage.MessageBody);
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO Message (ip, name, body, type) VALUES ('{userMessage.IP}', '{userMessage.Name}', '{body}', '{userMessage.Type}'";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static public List<UserMessage> SelectMessgae(string ip, string name)
        {
            List<UserMessage> userMessageList = new List<UserMessage>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT * FROM Message WHERE ip='{ip}' AND name='{name}' ORDER BY time DESC LIMIT 10";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        userMessageList.Add(new UserMessage()
                        {
                            IP = reader["ip"].ToString(),
                            Name = reader["name"].ToString(),
                            MessageBody = JsonConvert.DeserializeObject<List<UserMessageBody>>(reader["body"].ToString()),
                            Type = (MessageType)(int.Parse(reader["type"].ToString())),
                            Time = reader["time"].ToString(),
                        });
                    }
                }
            }
            return userMessageList;
        }
    }
}
