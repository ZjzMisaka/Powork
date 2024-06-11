﻿using Powork.Model;
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

                InsertTeamMembers(team.ID, team.MemberList);
            }
        }

        public static void InsertTeamMembers(string teamID, List<User> memberList)
        {
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
                            });
                        }
                    }
                }
            }
            return teamMemberList;
        }
    }
}