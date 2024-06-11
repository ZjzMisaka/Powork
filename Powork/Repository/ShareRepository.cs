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
    public static class ShareRepository
    {
        public static void InsertFile(ShareInfo shareInfo)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO TShare (id, path, name, extension, type, shareTime, createTime, lastModifiedTime) " +
                    $"VALUES ('{shareInfo.Guid}', '{shareInfo.Path}', '{shareInfo.Name}', '{shareInfo.Extension}', '{shareInfo.Type}', '{shareInfo.ShareTime}', '{shareInfo.CreateTime}', '{shareInfo.LastModifiedTime}')";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<ShareInfo> SelectFile()
        {
            List<ShareInfo> shareList = new List<ShareInfo>();
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = "SELECT * FROM TShare";

                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shareList.Add(new ShareInfo()
                            {
                                Guid = reader["id"].ToString(),
                                Path = reader["path"].ToString(),
                                Name = reader["name"].ToString(),
                                Extension = reader["extension"].ToString(),
                                Type = reader["type"].ToString(),
                                ShareTime = reader["shareTime"].ToString(),
                                CreateTime = reader["createTime"].ToString(),
                                LastModifiedTime = reader["lastModifiedTime"].ToString(),
                            });
                        }
                    }
                }
            }
            return shareList;
        }

        public static void InsertRemoteFile(ShareInfo shareInfo)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"INSERT INTO TShareRemote (id, path, name, extension, type, shareTime, createTime, lastModifiedTime) " +
                    $"VALUES ('{shareInfo.Guid}', '{shareInfo.Path}', '{shareInfo.Name}', '{shareInfo.Extension}', '{shareInfo.Type}', '{shareInfo.ShareTime}', '{shareInfo.CreateTime}', '{shareInfo.LastModifiedTime}')";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void ClearRemoteFile()
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"TRUNCATE TABLE TShareRemote";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
