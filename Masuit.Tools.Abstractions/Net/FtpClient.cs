using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Masuit.Tools.Net
{
    /// <summary>
    /// FTP客户端操作类
    /// </summary>
    public class FtpClient
    {
        #region 变量属性

        /// <summary>
        /// Ftp服务器ip
        /// </summary>
        private string FtpServer { get; set; }

        /// <summary>
        /// Ftp 指定用户名
        /// </summary>
        private string Username { get; set; }

        /// <summary>
        /// Ftp 指定用户密码
        /// </summary>
        private string Password { get; set; }

        #endregion

        /// <summary>
        /// 获取一个匿名登录的ftp客户端
        /// </summary>
        /// <param name="serverIp">服务器IP地址</param>
        /// <param name="matchInetAddress">是否验证IP地址</param>
        /// <returns></returns>
        public static FtpClient GetAnonymousClient(string serverIp,bool matchInetAddress=true)
        {
            if (!serverIp.MatchInetAddress()&&matchInetAddress)
            {
                throw new ArgumentException("IP地址格式不正确");
            }

            return new FtpClient
            {
                FtpServer = serverIp
            };
        }

        /// <summary>
        /// 获取一个匿名登录的ftp客户端
        /// </summary>
        /// <param name="serverIp">服务器ip</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="matchInetAddress">是否验证IP地址</param>
        /// <returns></returns>
        public static FtpClient GetClient(string serverIp, string username, string password,bool matchInetAddress=true)
        {
            if (!serverIp.MatchInetAddress()&&matchInetAddress)
            {
                throw new ArgumentException("IP地址格式不正确");
            }

            return new FtpClient
            {
                FtpServer = serverIp,
                Username = username,
                Password = password
            };
        }

        #region 从FTP服务器下载文件，指定本地路径和本地文件名

        /// <summary>
        /// 从FTP服务器下载文件，指定本地路径和本地文件名
        /// </summary>
        /// <param name="remoteFileName">远程文件名</param>
        /// <param name="localFileName">保存本地的文件名（包含路径）</param>
        /// <param name="ifCredential">是否启用身份验证（false：表示允许用户匿名下载）</param>
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>
        public void Download(string remoteFileName, string localFileName, bool ifCredential = false, Action<int, int> updateProgress = null)
        {
            using var outputStream = new FileStream(localFileName, FileMode.Create);
            if (FtpServer == null || FtpServer.Trim().Length == 0)
            {
                throw new Exception("ftp下载目标服务器地址未设置！");
            }

            Uri uri = new Uri("ftp://" + FtpServer + "/" + remoteFileName);
            var ftpsize = (FtpWebRequest)WebRequest.Create(uri);
            ftpsize.UseBinary = true;
            var reqFtp = (FtpWebRequest)WebRequest.Create(uri);
            reqFtp.UseBinary = true;
            reqFtp.KeepAlive = false;
            if (ifCredential) //使用用户身份认证
            {
                ftpsize.Credentials = new NetworkCredential(Username, Password);
                reqFtp.Credentials = new NetworkCredential(Username, Password);
            }

            ftpsize.Method = WebRequestMethods.Ftp.GetFileSize;
            using var re = (FtpWebResponse)ftpsize.GetResponse();
            long totalBytes = re.ContentLength;
            reqFtp.Method = WebRequestMethods.Ftp.DownloadFile;
            using var response = (FtpWebResponse)reqFtp.GetResponse();
            using var ftpStream = response.GetResponseStream();
            //更新进度 
            updateProgress?.Invoke((int)totalBytes, 0); //更新进度条 
            long totalDownloadedByte = 0;
            int bufferSize = 1024 * 1024;
            byte[] buffer = new byte[bufferSize];
            if (ftpStream != null)
            {
                var readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    totalDownloadedByte = readCount + totalDownloadedByte;
                    outputStream.Write(buffer, 0, readCount);
                    //更新进度 
                    updateProgress?.Invoke((int)totalBytes, (int)totalDownloadedByte); //更新进度条 
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
            }
        }

        /// <summary>
        /// 从FTP服务器下载文件，指定本地路径和本地文件名（支持断点下载）
        /// </summary>
        /// <param name="remoteFileName">远程文件名</param>
        /// <param name="localFileName">保存本地的文件名（包含路径）</param>
        /// <param name="ifCredential">是否启用身份验证（false：表示允许用户匿名下载）</param>
        /// <param name="size">已下载文件流大小</param>
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>
        public void BrokenDownload(string remoteFileName, string localFileName, bool ifCredential, long size, Action<int, int> updateProgress = null)
        {
            using var outputStream = new FileStream(localFileName, FileMode.Append);
            if (FtpServer == null || FtpServer.Trim().Length == 0)
            {
                throw new Exception("ftp下载目标服务器地址未设置！");
            }

            Uri uri = new Uri("ftp://" + FtpServer + "/" + remoteFileName);
            var ftpsize = (FtpWebRequest)WebRequest.Create(uri);
            ftpsize.UseBinary = true;
            ftpsize.ContentOffset = size;
            var reqFtp = (FtpWebRequest)WebRequest.Create(uri);
            reqFtp.UseBinary = true;
            reqFtp.KeepAlive = false;
            reqFtp.ContentOffset = size;
            if (ifCredential) //使用用户身份认证
            {
                ftpsize.Credentials = new NetworkCredential(Username, Password);
                reqFtp.Credentials = new NetworkCredential(Username, Password);
            }

            ftpsize.Method = WebRequestMethods.Ftp.GetFileSize;
            using var re = (FtpWebResponse)ftpsize.GetResponse();
            var totalBytes = re.ContentLength;
            reqFtp.Method = WebRequestMethods.Ftp.DownloadFile;
            using var response = (FtpWebResponse)reqFtp.GetResponse();
            using var ftpStream = response.GetResponseStream();
            updateProgress?.Invoke((int)totalBytes, 0); //更新进度条 
            long totalDownloadedByte = 0;
            int bufferSize = 1024 * 1024;
            byte[] buffer = new byte[bufferSize];
            if (ftpStream != null)
            {
                var readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    totalDownloadedByte = readCount + totalDownloadedByte;
                    outputStream.Write(buffer, 0, readCount);
                    //更新进度 
                    updateProgress?.Invoke((int)totalBytes, (int)totalDownloadedByte); //更新进度条 
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
            }
        }

        /// <summary>
        /// 从FTP服务器下载文件，指定本地路径和本地文件名
        /// </summary>
        /// <param name="remoteFileName">远程文件名</param>
        /// <param name="localFileName">保存本地的文件名（包含路径）</param>
        /// <param name="ifCredential">是否启用身份验证（false：表示允许用户匿名下载）</param>
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>
        /// <param name="brokenOpen">是否断点下载：true 会在localFileName 找是否存在已经下载的文件，并计算文件流大小</param>
        public void Download(string remoteFileName, string localFileName, bool ifCredential, bool brokenOpen, Action<int, int> updateProgress = null)
        {
            if (brokenOpen)
            {
                long size = 0;
                if (File.Exists(localFileName))
                {
                    using var outputStream = new FileStream(localFileName, FileMode.Open);
                    size = outputStream.Length;
                }

                BrokenDownload(remoteFileName, localFileName, ifCredential, size, updateProgress);
            }

            Download(remoteFileName, localFileName, ifCredential, updateProgress);
        }

        #endregion

        #region 上传文件到FTP服务器

        /// <summary>
        /// 上传文件到FTP服务器
        /// </summary>
        /// <param name="relativePath">相对目录</param>
        /// <param name="localFullPathName">本地带有完整路径的文件名</param>
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>
        public void UploadFile(string relativePath, string localFullPathName, Action<int, int> updateProgress = null)
        {
            var finfo = new FileInfo(localFullPathName);
            if (FtpServer == null || FtpServer.Trim().Length == 0)
            {
                throw new Exception("ftp上传目标服务器地址未设置！");
            }

            Uri uri = new Uri("ftp://" + FtpServer + "/" + relativePath + "/" + finfo.Name);
            var reqFtp = (FtpWebRequest)WebRequest.Create(uri);
            reqFtp.KeepAlive = false;
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(Username, Password); //用户，密码
            reqFtp.Method = WebRequestMethods.Ftp.UploadFile; //向服务器发出下载请求命令
            reqFtp.ContentLength = finfo.Length; //为request指定上传文件的大小
            int buffLength = 1024 * 1024;
            byte[] buff = new byte[buffLength];
            using var fs = finfo.OpenRead();
            using var stream = reqFtp.GetRequestStream();
            var contentLen = fs.Read(buff, 0, buffLength);
            int allbye = (int)finfo.Length;
            //更新进度 
            updateProgress?.Invoke(allbye, 0); //更新进度条 
            int startbye = 0;
            while (contentLen != 0)
            {
                startbye = contentLen + startbye;
                stream.Write(buff, 0, contentLen);
                //更新进度 
                updateProgress?.Invoke(allbye, startbye); //更新进度条 
                contentLen = fs.Read(buff, 0, buffLength);
            }
        }

        /// <summary>
        /// 上传文件到FTP服务器(断点续传)
        /// </summary>
        /// <param name="localFullPath">本地文件全路径名称：C:\Users\JianKunKing\Desktop\IronPython脚本测试工具</param>
        /// <param name="remoteFilepath">远程文件所在文件夹路径</param>
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>
        /// <returns></returns> 
        public bool UploadBroken(string localFullPath, string remoteFilepath, Action<int, int> updateProgress = null)
        {
            if (remoteFilepath == null)
            {
                remoteFilepath = "";
            }

            string newFileName;
            var fileInf = new FileInfo(localFullPath);
            long allbye = fileInf.Length;
            if (fileInf.Name.IndexOf("#", StringComparison.Ordinal) == -1)
            {
                newFileName = RemoveSpaces(fileInf.Name);
            }
            else
            {
                newFileName = fileInf.Name.Replace("#", "＃");
                newFileName = RemoveSpaces(newFileName);
            }

            long startfilesize = GetFileSize(newFileName, remoteFilepath);
            if (startfilesize >= allbye)
            {
                return false;
            }

            long startbye = startfilesize;
            //更新进度 
            updateProgress?.Invoke((int)allbye, (int)startfilesize); //更新进度条 
            string uri;
            if (remoteFilepath.Length == 0)
            {
                uri = "ftp://" + FtpServer + "/" + newFileName;
            }
            else
            {
                uri = "ftp://" + FtpServer + "/" + remoteFilepath + "/" + newFileName;
            }

            // 根据uri创建FtpWebRequest对象 
            var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(uri));
            // ftp用户名和密码 
            reqFtp.Credentials = new NetworkCredential(Username, Password);
            // 默认为true，连接不会被关闭 
            // 在一个命令之后被执行 
            reqFtp.KeepAlive = false;
            // 指定执行什么命令 
            reqFtp.Method = WebRequestMethods.Ftp.AppendFile;
            // 指定数据传输类型 
            reqFtp.UseBinary = true;
            // 上传文件时通知服务器文件的大小 
            reqFtp.ContentLength = fileInf.Length;
            int buffLength = 1024 * 1024; // 缓冲大小设置为2kb 
            byte[] buff = new byte[buffLength];
            // 打开一个文件流 (System.IO.FileStream) 去读上传的文件 
            using FileStream fs = fileInf.OpenRead();
            using var strm = reqFtp.GetRequestStream();
            // 把上传的文件写入流 
            fs.Seek(startfilesize, 0);
            int contentLen = fs.Read(buff, 0, buffLength);
            // 流内容没有结束 
            while (contentLen != 0)
            {
                // 把内容从file stream 写入 upload stream 
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
                startbye += contentLen;
                //更新进度 
                updateProgress?.Invoke((int)allbye, (int)startbye); //更新进度条 
            }

            return true;
        }

        /// <summary>
        /// 去除空格
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string RemoveSpaces(string str)
        {
            string a = str.Where(c => Encoding.ASCII.GetBytes(c.ToString())[0] != 32).Aggregate("", (current, c) => current + c);
            return a.Split('.')[a.Split('.').Length - 2] + "." + a.Split('.')[a.Split('.').Length - 1];
        }

        /// <summary>
        /// 获取已上传文件大小
        /// </summary>
        /// <param name="filePath">文件名称</param>
        /// <param name="remoteFilepath">服务器文件路径</param>
        /// <returns></returns>
        public long GetFileSize(string filePath, string remoteFilepath)
        {
            try
            {
                var fi = new FileInfo(filePath);
                string uri;
                if (remoteFilepath.Length == 0)
                {
                    uri = "ftp://" + FtpServer + "/" + fi.Name;
                }
                else
                {
                    uri = "ftp://" + FtpServer + "/" + remoteFilepath + "/" + fi.Name;
                }

                var reqFtp = (FtpWebRequest)WebRequest.Create(uri);
                reqFtp.KeepAlive = false;
                reqFtp.UseBinary = true;
                reqFtp.Credentials = new NetworkCredential(Username, Password); //用户，密码
                reqFtp.Method = WebRequestMethods.Ftp.GetFileSize;
                var response = (FtpWebResponse)reqFtp.GetResponse();
                var filesize = response.ContentLength;
                return filesize;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region 获取当前目录下明细

        /// <summary>
        /// 获取当前目录下明细(包含文件和文件夹)
        /// </summary>
        /// <returns></returns>
        public List<string> GetFilesDetails(string relativePath = "")
        {
            var result = new List<string>();
            var ftp = (FtpWebRequest)WebRequest.Create(new Uri(Path.Combine("ftp://" + FtpServer, relativePath).Replace("\\", "/")));
            ftp.Credentials = new NetworkCredential(Username, Password);
            ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            using var response = ftp.GetResponse();
            using var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8);
            string line = reader.ReadLine();
            while (line != null)
            {
                result.Add(line);
                line = reader.ReadLine();
            }

            return result;
        }

        /// <summary>
        /// 获取当前目录下文件列表(仅文件)
        /// </summary>
        /// <returns></returns>
        public List<string> GetFiles(string relativePath = "", string mask = "*.*")
        {
            var result = new List<string>();
            var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(Path.Combine("ftp://" + FtpServer, relativePath).Replace("\\", "/")));
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(Username, Password);
            reqFtp.Method = WebRequestMethods.Ftp.ListDirectory;
            using var response = reqFtp.GetResponse();
            using var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8);
            string line = reader.ReadLine();
            while (line != null)
            {
                if (mask.Trim() != string.Empty && mask.Trim() != "*.*")
                {
                    string temp = mask.Substring(0, mask.IndexOf("*", StringComparison.Ordinal));
                    if (line.Substring(0, temp.Length) == temp)
                    {
                        result.Add(line);
                    }
                }
                else
                {
                    result.Add(line);
                }

                line = reader.ReadLine();
            }

            return result;
        }

        /// <summary>
        /// 获取当前目录下所有的文件夹列表(仅文件夹)
        /// </summary>
        /// <returns></returns>
        public string[] GetDirectories(string relativePath)
        {
            var drectory = GetFilesDetails(relativePath);
            string m = string.Empty;
            foreach (string str in drectory)
            {
                int dirPos = str.IndexOf("<DIR>", StringComparison.Ordinal);
                if (dirPos > 0)
                {
                    /*判断 Windows 风格*/
                    m += str.Substring(dirPos + 5).Trim() + "\n";
                }
                else if (str.Trim().StartsWith("d"))
                {
                    /*判断 Unix 风格*/
                    string dir = str.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[8];
                    if (dir != "." && dir != "..")
                    {
                        dir = str.Substring(str.IndexOf(dir, StringComparison.Ordinal));
                        m += dir + "\n";
                    }
                }
            }

            char[] n =
            {
                '\n'
            };
            return m.Split(n);
        }
        #endregion

        #region 删除文件及文件夹

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath"></param>
        public void Delete(string filePath)
        {
            string uri = Path.Combine("ftp://" + FtpServer, filePath).Replace("\\", "/");
            var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(uri));
            reqFtp.Credentials = new NetworkCredential(Username, Password);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.DeleteFile;
            using FtpWebResponse response = (FtpWebResponse)reqFtp.GetResponse();
            using Stream datastream = response.GetResponseStream();
            using StreamReader sr = new StreamReader(datastream ?? throw new InvalidOperationException());
            sr.ReadToEnd();
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="dirPath"></param>
        public void RemoveDirectory(string dirPath)
        {
            string uri = Path.Combine("ftp://" + FtpServer, dirPath).Replace("\\", "/");
            var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(uri));
            reqFtp.Credentials = new NetworkCredential(Username, Password);
            reqFtp.KeepAlive = false;
            reqFtp.Method = WebRequestMethods.Ftp.RemoveDirectory;
            using var response = (FtpWebResponse)reqFtp.GetResponse();
            using var datastream = response.GetResponseStream();
            using var sr = new StreamReader(datastream ?? throw new InvalidOperationException());
            sr.ReadToEnd();
        }

        #endregion

        #region 其他操作

        /// <summary>
        /// 获取指定文件大小
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public long GetFileSize(string filePath)
        {
            var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(Path.Combine("ftp://" + FtpServer, filePath).Replace("\\", "/")));
            reqFtp.Method = WebRequestMethods.Ftp.GetFileSize;
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(Username, Password);
            using var response = (FtpWebResponse)reqFtp.GetResponse();
            var fileSize = response.ContentLength;
            return fileSize;
        }

        /// <summary>
        /// 判断当前目录下指定的子目录是否存在
        /// </summary>
        /// <param name="remoteDirPath">指定的目录名</param>
        public bool DirectoryExist(string remoteDirPath)
        {
            try
            {
                string[] dirList = GetDirectories(remoteDirPath);
                return dirList.Any(str => str.Trim() == remoteDirPath.Trim());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 判断当前目录下指定的文件是否存在
        /// </summary>
        /// <param name="remoteFileName">远程文件名</param>
        public bool FileExist(string remoteFileName)
        {
            return GetFiles("*.*").Any(str => str.Trim() == remoteFileName.Trim());
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="relativePath">路径</param>
        /// <param name="newDir">新建文件夹</param>
        public void MakeDir(string relativePath, string newDir)
        {
            var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(Path.Combine("ftp://" + FtpServer, relativePath, newDir).Replace("\\", "/")));
            reqFtp.Method = WebRequestMethods.Ftp.MakeDirectory;
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(Username, Password);
            using var response = (FtpWebResponse)reqFtp.GetResponse();
            using var _ = response.GetResponseStream();
        }

        /// <summary>
        /// 改名
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="currentFilename"></param>
        /// <param name="newFilename"></param>
        public void Rename(string relativePath, string currentFilename, string newFilename)
        {
            var reqFtp = (FtpWebRequest)WebRequest.Create(new Uri(Path.Combine("ftp://" + FtpServer, relativePath, currentFilename).Replace("\\", "/")));
            reqFtp.Method = WebRequestMethods.Ftp.Rename;
            reqFtp.RenameTo = newFilename;
            reqFtp.UseBinary = true;
            reqFtp.Credentials = new NetworkCredential(Username, Password);
            using var response = (FtpWebResponse)reqFtp.GetResponse();
            using var _ = response.GetResponseStream();
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="currentFilename"></param>
        /// <param name="newDirectory"></param>
        public void MoveFile(string relativePath, string currentFilename, string newDirectory)
        {
            Rename(relativePath, currentFilename, newDirectory);
        }
        #endregion
    }
}