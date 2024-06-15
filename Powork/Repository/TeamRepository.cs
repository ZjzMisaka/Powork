using System.Data.SQLite;
using Powork.Model;

namespace Powork.Repository
{
    public static class TeamRepository
    {
        public static void InsertOrUpdateTeam(Team team)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT OR REPLACE INTO TTeam (id, name, lastModifiedTime) VALUES ('{team.ID}', '{team.Name}', '{team.LastModifiedTime}')";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }

                InsertOrUpdateTeamMembers(team.ID, team.MemberList);
            }
        }

        public static void RemoveTeam(string id)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sqlTTeam = $"DELETE FROM TTeam WHERE id = '{id}'";
                string sqlTTeamMember = $"DELETE FROM TTeamMember WHERE teamID = '{id}'";

                using (var command = new SQLiteCommand(sqlTTeam, connection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand(sqlTTeamMember, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void InsertOrUpdateTeamMembers(string teamID, List<User> memberList)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                foreach (User user in memberList)
                {
                    string sql = $"INSERT OR REPLACE INTO TTeamMember (teamID, userIP, userName, groupName) VALUES ('{teamID}', '{user.IP}', '{user.Name}', '{user.GroupName}')";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static List<Team> SelectTeam()
        {
            List<Team> teamList = new List<Team>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = "SELECT * FROM TTeam";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            teamList.Add(new Team()
                            {
                                ID = reader["id"].ToString(),
                                Name = reader["name"].ToString(),
                                LastModifiedTime = reader["lastModifiedTime"].ToString(),
                            });
                        }
                    }
                }
            }
            return teamList;
        }

        public static Team SelectTeam(string id)
        {
            Team team = null;
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT * FROM TTeam WHERE id = '{id}'";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            team = new Team()
                            {
                                ID = reader["id"].ToString(),
                                Name = reader["name"].ToString(),
                                LastModifiedTime = reader["lastModifiedTime"].ToString(),
                            };
                        }
                    }
                }
            }
            return team;
        }

        public static List<User> SelectTeamMember(string teamID)
        {
            List<User> teamMemberList = new List<User>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT * FROM TTeamMember WHERE teamID = '{teamID}'";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            teamMemberList.Add(new User()
                            {
                                IP = reader["userIP"].ToString(),
                                Name = reader["userName"].ToString(),
                                GroupName = reader["groupName"].ToString(),
                            });
                        }
                    }
                }
            }
            return teamMemberList;
        }

        public static bool ContainMember(string teamID, string userIP, string userName)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT COUNT(*) FROM TTeamMember WHERE teamID = '{teamID}' AND userIP = '{userIP}' AND userName = '{userName}'";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int count = reader.GetInt32(0);
                            if (count >= 1)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
