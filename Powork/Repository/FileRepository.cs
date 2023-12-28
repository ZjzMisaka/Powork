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
    public static class FileRepository
    {
        public static void InsertFile(string guid, string path)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO TFile (id, path) VALUES ('{guid}', '{path}')";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static string SelectFile(string guid)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"SELECT * FROM TFile WHERE id = '{guid}'";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["path"].ToString();
                        }
                    }
                }
            }
            return null;
        }
    }
}
