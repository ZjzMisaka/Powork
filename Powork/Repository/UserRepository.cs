using System.Data.SQLite;
using Powork.Model;

namespace Powork.Repository
{
    public static class UserRepository
    {
        public static void InsertUser(User user)
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

        public static List<User> SelectUser()
        {
            List<User> userList = new List<User>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = "SELECT * FROM TUser";
                List<User> selfInfo = GlobalVariables.SelfInfo;
                if (selfInfo != null && selfInfo.Count == 1)
                {
                    sql = $"{sql} WHERE ip <> '{selfInfo[0].IP}' AND name <> '{selfInfo[0].Name}'";
                }

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

        public static void UpdateUserByIp(User user)
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

        public static List<User> SelectUserByIp(string ip)
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

        public static List<User> SelectUserByIpName(string ip, string name)
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

        public static void RemoveUserByIp(string ip)
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

        public static void RemoveUser(string ip, string name)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"DELETE FROM TUser WHERE ip = '{ip}' AND  name = '{name}'";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
