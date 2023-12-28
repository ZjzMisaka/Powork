using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Powork.Helper
{
    public static class FileHelper
    {
        public enum Type
        { 
            File,
            Image,
            Directory,
            None,
        }
        public static Type GetType(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            if (!Directory.Exists(path) && !File.Exists(path))
            {
                return Type.None;
            }

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return Type.Directory;
            }
            else
            {
                string extension = Path.GetExtension(path).ToLower();
                if (extension == ".png" || extension == ".jpg" || extension == ".bmp")
                {
                    return Type.Image;
                }
                else
                {
                    return Type.File;
                }
            }
        }

        public static string GetRelativePath(string fullPath, string rootPath)
        {
            if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                rootPath += Path.DirectorySeparatorChar;
            }

            string directoryPath = Path.GetDirectoryName(fullPath);

            Uri fileUri = new Uri(directoryPath);
            Uri rootUri = new Uri(rootPath);

            Uri relativeUri = rootUri.MakeRelativeUri(fileUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);

            return relativePath;
        }
    }
}
