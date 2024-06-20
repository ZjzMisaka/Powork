using System.Data.SQLite;
using Powork.Constant;
using Powork.Helper;
using Powork.Model;

namespace Powork.Repository
{
    public static class ShareRepository
    {
        public static void InsertFile(ShareInfo shareInfo)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"INSERT INTO TShare (id, path, name, extension, type, size, shareTime, createTime, lastModifiedTime) " +
                    $"VALUES ('{shareInfo.Guid}', '{shareInfo.Path}', '{shareInfo.Name}', '{shareInfo.Extension}', '{shareInfo.Type}', '{shareInfo.Size}', '{shareInfo.ShareTime}', '{shareInfo.CreateTime}', '{shareInfo.LastModifiedTime}')";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }

            FileRepository.InsertFile(shareInfo.Guid, shareInfo.Path);
        }

        public static List<ShareInfo> SelectFile()
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            List<ShareInfo> shareList = new List<ShareInfo>();

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
                            ShareTime = DateTime.Parse(reader["shareTime"].ToString()).ToString(Format.DateTimeFormatWithSeconds),
                            CreateTime = DateTime.Parse(reader["createTime"].ToString()).ToString(Format.DateTimeFormatWithSeconds),
                            LastModifiedTime = DateTime.Parse(reader["lastModifiedTime"].ToString()).ToString(Format.DateTimeFormatWithSeconds),
                        });
                    }
                }
            }

            return shareList;
        }

        public static void UpdateFile(ShareInfo shareInfo)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"UPDATE TShare SET size = '{shareInfo.Size}', lastModifiedTime = '{shareInfo.LastModifiedTime}' WHERE id = '{shareInfo.Guid}'";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void RemoveFile(string id)
        {
            SQLiteConnection connection = CommonRepository.GetConnection();

            string sql = $"DELETE FROM TShare WHERE id = '{id}'";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
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
                    string lastModifiedTime = fileInfo.LastWriteTime.ToString(Format.DateTimeFormatWithMilliseconds);
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
                    string lastModifiedTime = directoryInfo.LastWriteTime.ToString(Format.DateTimeFormatWithMilliseconds);
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
