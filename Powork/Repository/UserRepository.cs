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
    static internal class UserRepository
    {
        static public void InsertUser(User user)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO TUser (ip, name, groupName) VALUES ('{user.IP}', '{user.Name}', '{user.GroupName}')";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static public List<User> SelectUser()
        {
            List<User> userList = new List<User>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = "SELECT * FROM TUser";

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

        static public void UpdateUserByIp(User user)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"UPDATE TUser SET name = '{user.Name}', groupName = '{user.GroupName}' WHERE ip = '{user.IP}'";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static public List<User> SelectUserByIp(string ip)
        {
            List<User> userList = new List<User>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT ip, name, groupName FROM TUser WHERE ip = '{ip}'";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User()
                            {
                                IP = reader["ip"].ToString(),
                                Name = reader["name"].ToString(),
                                GroupName = reader["groupName"].ToString()
                            };
                            userList.Add(user);
                        }
                    }
                }

                return userList;
            }
        }

        static public List<User> SelectUserByIpName(string ip, string name)
        {
            List<User> userList = new List<User>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT ip, name, groupName FROM TUser WHERE ip = '{ip}' AND name = '{name}'";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User()
                            {
                                IP = reader["ip"].ToString(),
                                Name = reader["name"].ToString(),
                                GroupName = reader["groupName"].ToString()
                            };
                            userList.Add(user);
                        }
                    }
                }

                return userList;
            }
        }

        static public void RemoveUserByIp(string ip)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"DELETE FROM TUser WHERE ip = '{ip}'";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
