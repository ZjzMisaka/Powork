using System.Data.SQLite;
using Powork.Model;

namespace Powork.Repository
{
    public static class UserRepository
    {
        public static void InsertUser(User user)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"INSERT INTO TUser (ip, name, groupName) VALUES ('{user.IP}', '{user.Name}', '{user.GroupName}')";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static List<User> SelectUser()
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            List<User> userList = new List<User>();

            string sql = "SELECT * FROM TUser";
            User selfInfo = GlobalVariables.SelfInfo;
            if (selfInfo != null && selfInfo != null)
            {
                sql = $"{sql} WHERE ip <> '{selfInfo.IP}' AND name <> '{selfInfo.Name}'";
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
            return userList;
        }

        public static void UpdateUserByIpName(User user)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"UPDATE TUser SET groupName = '{user.GroupName}' WHERE ip = '{user.IP}' AND name = '{user.Name}'";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static List<User> SelectUserByIp(string ip)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            List<User> userList = new List<User>();

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

        public static List<User> SelectUserByIpName(string ip, string name)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            List<User> userList = new List<User>();

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

        public static void RemoveUserByIp(string ip)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"DELETE FROM TUser WHERE ip = '{ip}'";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveUser(string ip, string name)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"DELETE FROM TUser WHERE ip = '{ip}' AND  name = '{name}'";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void InsertLogonUser(User user)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"INSERT INTO TMe (ip, name, groupName) VALUES ('{user.IP}', '{user.Name}', '{user.GroupName}')";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void UpdateLogonUserByIP(User user)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"UPDATE TMe SET name = '{user.Name}', groupName = '{user.GroupName}' WHERE ip = '{user.IP}'";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static User SelectLogonUserCurrentIP()
        {
            string ip = GlobalVariables.LocalIP.ToString();
            List<User> userList = SelectLogonUser(ip);
            if (userList.Count == 1)
            {
                return userList[0];
            }
            else
            {
                return null;
            }
        }

        public static List<User> SelectLogonUser(string ip = null)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            List<User> userList = new List<User>();
            string sql = $"SELECT ip, name, groupName FROM TMe";
            if (ip != null)
            {
                sql += $" WHERE ip = '{ip}'";
            }

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

        public static bool IsSelf(string ip, string name)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"SELECT COUNT(*) FROM TMe WHERE ip = '{ip}' AND name = '{name}'";

            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int count = reader.GetInt32(0);
                        if (count != 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
