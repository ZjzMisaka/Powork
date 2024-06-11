using Powork.Helper;
using Powork.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
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

                string sql = $"INSERT INTO TShare (id, path, name, extension, type, size, shareTime, createTime, lastModifiedTime) " +
                    $"VALUES ('{shareInfo.Guid}', '{shareInfo.Path}', '{shareInfo.Name}', '{shareInfo.Extension}', '{shareInfo.Type}', '{shareInfo.Size}', '{shareInfo.ShareTime}', '{shareInfo.CreateTime}', '{shareInfo.LastModifiedTime}')";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            FileRepository.InsertFile(shareInfo.Guid, shareInfo.Path);
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
                                Size = reader["size"].ToString(),
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

        public static void UpdateFile(ShareInfo shareInfo)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"UPDATE TShare SET size = '{shareInfo.Size}', lastModifiedTime = '{shareInfo.LastModifiedTime}' WHERE id = '{shareInfo.Guid}'";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            FileRepository.InsertFile(shareInfo.Guid, shareInfo.Path);
        }

        public static void RemoveFile(string id)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVariables.DbName};Version=3;"))
            {
                connection.Open();

                string sql = $"DELETE FROM TShare WHERE id = '{id}'";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            FileRepository.RemoveFile(id);
        }

        public static void ReCheckFiles()
        {
            List<ShareInfo> shareList = SelectFile();
            foreach (ShareInfo shareInfo in shareList)
            {
                if (shareInfo.Type == "File")
                {
                    if (!System.IO.File.Exists(shareInfo.Path))
                    {
                        RemoveFile(shareInfo.Guid);
                        continue;
                    }

                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(shareInfo.Path);
                    string lastModifiedTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string size = FileHelper.GetReadableFileSize(fileInfo.Length);
                    if (shareInfo.LastModifiedTime != lastModifiedTime || shareInfo.Size != size)
                    {
                        shareInfo.LastModifiedTime = lastModifiedTime;
                        shareInfo.Size = size;
                        UpdateFile(shareInfo);
                    }
                }
                else
                {
                    if (!System.IO.Directory.Exists(shareInfo.Path))
                    {
                        RemoveFile(shareInfo.Guid);
                        continue;
                    }

                    System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(shareInfo.Path);
                    string lastModifiedTime = directoryInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    if (shareInfo.LastModifiedTime != lastModifiedTime)
                    {
                        shareInfo.LastModifiedTime = lastModifiedTime;
                        UpdateFile(shareInfo);
                    }
                }
            }
        }
    }
}
