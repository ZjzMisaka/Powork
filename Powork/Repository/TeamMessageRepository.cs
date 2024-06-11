﻿using Newtonsoft.Json;
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
    public class TeamMessageRepository
    {
        public static void InsertMessage(TCPMessage userMessage)
        {
            string body = JsonConvert.SerializeObject(userMessage.MessageBody);
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO TMessage (body, type, time, fromIP, fromName, toIP, teamID, toName) VALUES (@body, @type, @time, @fromIP, @fromName, @toIP, @teamID, @toName)";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.Add(new SQLiteParameter("@body", body));
                    command.Parameters.Add(new SQLiteParameter("@type", userMessage.Type));
                    command.Parameters.Add(new SQLiteParameter("@time", userMessage.Time));
                    command.Parameters.Add(new SQLiteParameter("@fromIP", userMessage.SenderIP));
                    command.Parameters.Add(new SQLiteParameter("@fromName", userMessage.SenderName));
                    command.Parameters.Add(new SQLiteParameter("@toIP", null));
                    command.Parameters.Add(new SQLiteParameter("@teamID", userMessage.TeamID));
                    command.Parameters.Add(new SQLiteParameter("@toName", null));
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<TCPMessage> SelectMessgae(string teamID, int id = -1)
        {
            List<TCPMessage> userMessageList = new List<TCPMessage>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT * FROM TMessage WHERE teamID = '{teamID}'";
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
                            userMessageList.Add(new TCPMessage()
                            {
                                SenderIP = reader["fromIP"].ToString(),
                                SenderName = reader["fromName"].ToString(),
                                MessageBody = JsonConvert.DeserializeObject<List<TCPMessageBody>>(reader["body"].ToString()),
                                Type = (MessageType)(int.Parse(reader["type"].ToString())),
                                Time = reader["time"].ToString(),
                                ID = int.Parse(reader["id"].ToString()),
                                TeamID = teamID,
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