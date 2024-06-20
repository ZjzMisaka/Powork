﻿using System.Data.SQLite;
using System.IO;

namespace Powork.Repository
{
    public static class CommonRepository
    {
        public static void CreateDatabase()
        {
            if (!File.Exists(GlobalVariables.DbName))
            {
                SQLiteConnection.CreateFile(GlobalVariables.DbName);
            }
        }

        public static void CreateTable()
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sqlTMe = @"CREATE TABLE IF NOT EXISTS TMe (ip VARCHAR(15), name VARCHAR(20), groupName VARCHAR(20), primary key (ip, name))";
                using (var command = new SQLiteCommand(sqlTMe, connection))
                {
                    command.ExecuteNonQuery();
                }

                string sqlTUser = @"CREATE TABLE IF NOT EXISTS TUser (ip VARCHAR(15), name VARCHAR(20), groupName VARCHAR(20), primary key (ip, name))";
                using (var command = new SQLiteCommand(sqlTUser, connection))
                {
                    command.ExecuteNonQuery();
                }

                string sqlTTeam = @"CREATE TABLE IF NOT EXISTS TTeam (id VARCHAR(36), name VARCHAR(20), lastModifiedTime DATETIME, primary key (id))";
                using (var command = new SQLiteCommand(sqlTTeam, connection))
                {
                    command.ExecuteNonQuery();
                }

                string sqlTTeamMember = @"CREATE TABLE IF NOT EXISTS TTeamMember (teamID VARCHAR(36), userIP VARCHAR(15), userName VARCHAR(20), groupName VARCHAR(20), primary key (teamID, userIP, userName))";
                using (var command = new SQLiteCommand(sqlTTeamMember, connection))
                {
                    command.ExecuteNonQuery();
                }

                string sqlTUserMessage = @"CREATE TABLE IF NOT EXISTS TUserMessage (body VARCHAR(5000), type INTEGER, time TIMESTAMP DEFAULT CURRENT_TIMESTAMP, fromIP VARCHAR(36), fromName VARCHAR(20), toIP VARCHAR(36), toName VARCHAR(20), id INTEGER PRIMARY KEY AUTOINCREMENT)";
                using (var command = new SQLiteCommand(sqlTUserMessage, connection))
                {
                    command.ExecuteNonQuery();
                }

                string sqlTTeamMessage = @"CREATE TABLE IF NOT EXISTS TTeamMessage (body VARCHAR(5000), type INTEGER, time TIMESTAMP DEFAULT CURRENT_TIMESTAMP, fromIP VARCHAR(36), fromName VARCHAR(20), teamID VARCHAR(36), id INTEGER PRIMARY KEY AUTOINCREMENT)";
                using (var command = new SQLiteCommand(sqlTTeamMessage, connection))
                {
                    command.ExecuteNonQuery();
                }

                string sqlTFile = @"CREATE TABLE IF NOT EXISTS TFile (id VARCHAR(36) PRIMARY KEY, path VARCHAR(256))";
                using (var command = new SQLiteCommand(sqlTFile, connection))
                {
                    command.ExecuteNonQuery();
                }

                string sqlTShare = @"CREATE TABLE IF NOT EXISTS TShare (id VARCHAR(36) PRIMARY KEY, path VARCHAR(256), name VARCHAR(256), extension VARCHAR(10), type VARCHAR(10), size VARCHAR(8), shareTime DATETIME, createTime DATETIME, lastModifiedTime DATETIME)";
                using (var command = new SQLiteCommand(sqlTShare, connection))
                {
                    command.ExecuteNonQuery();
                }

                string sqlTMemo = @"CREATE TABLE IF NOT EXISTS TMemo (date DATE DEFAULT (date('now')) PRIMARY KEY, memo TEXT)";
                using (var command = new SQLiteCommand(sqlTMemo, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
