using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;

namespace Common.Utility
{
    public static class FileUploadManager
    {
        public static bool FileExists(string fileIdentity)
        {
            return GetFilePhysicalFullPath(fileIdentity) != null;
        }

        public static FileStream OpenFile(string fileIdentity)
        {
            return OpenFile(fileIdentity, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public static FileStream OpenFile(string fileIdentity, FileMode mode, FileAccess access, FileShare share)
        {
            string filePath = GetFilePhysicalFullPath(fileIdentity);
            if (filePath == null)
            {
                return null;
            }
            return new FileStream(filePath, mode, access, share);
        }

        public static byte[] GetFileData(string fileIdentity, int offset, long length)
        {
            long total = length;
            using (FileStream fs = OpenFile(fileIdentity))
            {
                if (fs == null)
                {
                    return null;
                }
                fs.Seek(offset, SeekOrigin.Begin);
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] buffer = new byte[4096];
                    long tempTotal = 0;
                    while (tempTotal < total)
                    {
                        long bytesToRead = total - tempTotal;
                        if (bytesToRead > buffer.Length)
                        {
                            bytesToRead = buffer.Length;
                        }
                        int x = fs.Read(buffer, 0, (int)bytesToRead);
                        if (x <= 0)
                        {
                            break;
                        }
                        ms.Write(buffer, 0, (int)x);
                        tempTotal += x;
                    }
                    fs.Close();
                    return ms.ToArray();
                }
            }
        }

        public static byte[] GetFileData(string fileIdentity)
        {
            using (FileStream fs = OpenFile(fileIdentity))
            {
                if (fs == null)
                {
                    return null;
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                    }
                    fs.Close();
                    return ms.ToArray();
                }
            }
        }

        public static void DeleteFile(string fileIdentity)
        {
            string filePath = GetFilePhysicalFullPath(fileIdentity);
            if (filePath != null)
            {
                File.Delete(filePath);
            }
        }

        public static string GetFileExtensionName(string fileIdentity)
        {
            string path = GetFilePhysicalFullPath(fileIdentity);
            if (path == null)
            {
                return null;
            }
            return Path.GetExtension(path);
        }

        public static bool MoveFile(string fileIdentity, string destinationFileName)
        {
            string path = GetFilePhysicalFullPath(fileIdentity);
            if (path == null)
            {
                return false;
            }
            File.Move(path, destinationFileName);
            return true;
        }

        private static string GetFilePhysicalFullPath(string fileIdentity)
        {
            if (fileIdentity == null || fileIdentity.Trim().Length <= 0)
            {
                return null;
            }
            fileIdentity = Encoding.UTF8.GetString(Convert.FromBase64String(fileIdentity));
            string filePath = Path.Combine(BaseFolder, fileIdentity.Trim());
            if (!File.Exists(filePath))
            {
                return null;
            }
            return filePath;
        }

        public static string BaseFolder
        {
            get
            {
                string folder = ConfigurationManager.AppSettings["UploadFileBaseFolder"];
                if (folder == null || folder.Trim().Length <= 0)
                {
                    return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "UploadFiles");
                }
                folder = folder.Trim();
                string p = Path.GetPathRoot(folder);
                if (p == null || p.Trim().Length <= 0) // 说明是相对路径
                {
                    return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, folder);
                }
                return folder;
            }
        }
    }

    public class FileUploadHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string fileIdentity = context.Request.QueryString["FileIdentity"];
            if (fileIdentity == null || fileIdentity.Trim().Length <= 0)
            {
                throw new ApplicationException("There is no 'FileIdentity'.");
            }
            fileIdentity = Encoding.UTF8.GetString(Convert.FromBase64String(fileIdentity));
            if (context.Request.QueryString["Action"] == "cancel")
            {
                FileUploadManager.DeleteFile(fileIdentity);
                return;
            }
            long startPosition = 0;
            string startStr = context.Request.QueryString["StartPosition"];
            if (startStr == null || startStr.Trim().Length <= 0 || !long.TryParse(startStr, out startPosition) || startPosition < 0)
            {
                throw new ApplicationException("There is no 'StartPosition' or 'StartPosition' is invalid.");
            }

            string filePath = Path.Combine(FileUploadManager.BaseFolder, fileIdentity);
            string folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            Stream stream = context.Request.InputStream;
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                fs.Seek(startPosition, SeekOrigin.Begin);
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                }
                fs.Close();
            }
            context.Response.Write(fileIdentity);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
