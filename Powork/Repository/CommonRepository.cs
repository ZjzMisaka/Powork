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

namespace Powork.Repository
{
    static internal class CommonRepository
    {
        static public void CreateDatabase()
        {
            if (!File.Exists(GlobalVariables.DbName))
            {
                SQLiteConnection.CreateFile(GlobalVariables.DbName);
            }
        }

        static public void CreateTable()
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
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
    }
}
