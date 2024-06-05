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
    public static class TeamRepository
    {
        public static void InsertTeam(Team team)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO TTeam (id, name) VALUES ('{team.ID}', '{team.Name}')";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }

                SelectTeamMembers(team.ID, team.MemberList);
            }
        }

        public static List<User> SelectTeamMembers(string teamID, List<User> memberList)
        {
            List<User> userList = new List<User>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                foreach (User user in memberList)
                {
                    string sql = $"INSERT INTO TTeamMember (teamID, userIP, userName) VALUES ('{teamID}', '{user.IP}', '{user.Name}')";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            return userList;
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
                            });
                        }
                    }
                }
            }
            return teamList;
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
                            });
                        }
                    }
                }
            }
            return teamMemberList;
        }
    }
}
