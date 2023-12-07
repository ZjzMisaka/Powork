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

                string sqlTMessage = @"CREATE TABLE IF NOT EXISTS TMessage (ip VARCHAR(15), name VARCHAR(20), id VARCHAR(36), content VARCHAR(5000), type VARCHAR(4), time TIMESTAMP DEFAULT CURRENT_TIMESTAMP)";
                using (var command = new SQLiteCommand(sqlTMessage, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static public void InsertUser(string ip, string name, string groupName)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO Users (ip, name, groupName) VALUES ('{ip}', '{name}', '{groupName}')";

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

        static public void InsertMessage(string ip, string name, string id, string content, string type)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO Message (ip, name, id, content, type) VALUES ('{ip}', '{name}', '{id}', '{content}', '{type}'";

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
                            ID = reader["id"].ToString(),
                            Content = reader["content"].ToString(),
                            Type = reader["type"].ToString() == "TEXT" ? MessageType.Text : MessageType.File,
                            Time = reader["time"].ToString(),
                        });
                    }
                }
            }
            return userMessageList;
        }
    }
}
