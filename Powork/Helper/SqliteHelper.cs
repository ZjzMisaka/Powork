using Newtonsoft.Json;
using Powork.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Helper
{
    static internal class SqliteHelper
    {
        const string dbName = "PoworkDB";
        static public void CreateDatabase()
        {
            if (!File.Exists(dbName))
            {
                SQLiteConnection.CreateFile(dbName);
            }
        }

        static public void CreateTable()
        {
            using (var connection = new SQLiteConnection($"Data Source={dbName};Version=3;"))
            {
                connection.Open();

                string sqlTUser = @"CREATE TABLE IF NOT EXISTS TUser (ip VARCHAR(15), name VARCHAR(20), groupName VARCHAR(20))";
                using (var command = new SQLiteCommand(sqlTUser, connection))
                {
                    command.ExecuteNonQuery();
                }

                string sqlTMessage = @"CREATE TABLE IF NOT EXISTS TMessage (ip VARCHAR(15), name VARCHAR(20), id VARCHAR(36), body VARCHAR(5000), type INT, time TIMESTAMP DEFAULT CURRENT_TIMESTAMP)";
                using (var command = new SQLiteCommand(sqlTMessage, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static public void InsertUser(User user)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO Users (ip, name, groupName) VALUES ('{user.IP}', '{user.Name}', '{user.GroupName}')";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static public List<User> SelectUser(string dbName)
        {
            List<User> userList = new List<User>();
            using (var connection = new SQLiteConnection($"Data Source={dbName};Version=3;"))
            {
                connection.Open();

                string sql = "SELECT * FROM Users";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userList.Add(new User() 
                            {
                                IP = reader["ip"].ToString(), 
                                Name = reader["name"].ToString(),
                                GroupName = reader["groupName"].ToString()
                            });
                        }
                    }
                }
            }
            return userList;
        }

        static public void InsertMessage(UserMessage userMessage)
        {
            string body = JsonConvert.SerializeObject(userMessage.MessageBody);
            using (var connection = new SQLiteConnection($"Data Source={dbName};Version=3;"))
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
            using (var connection = new SQLiteConnection($"Data Source={dbName};Version=3;"))
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
