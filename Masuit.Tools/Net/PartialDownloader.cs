using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace Masuit.Tools.Net
{
    /// <summary>
    /// 部分下载器
    /// </summary>
    public class PartialDownloader
    {
        #region Variables

        /// <summary>
        /// 这部分完成事件
        /// </summary>
        public event EventHandler DownloadPartCompleted;

        /// <summary>
        /// 部分下载进度改变事件
        /// </summary>
        public event EventHandler DownloadPartProgressChanged;

        /// <summary>
        /// 部分下载停止事件
        /// </summary>
        public event EventHandler DownloadPartStopped;

        HttpWebRequest _req;
        HttpWebResponse _resp;
        Stream _tempStream;
        FileStream _file;
        private readonly AsyncOperation _aop = AsyncOperationManager.CreateOperation(null);
        private readonly Stopwatch _stp;
        readonly int[] _lastSpeeds;
        int _counter;
        bool _stop, _wait;

        #endregion

        #region PartialDownloader

        /// <summary>
        /// 部分块下载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dir"></param>
        /// <param name="fileGuid"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="rangeAllowed"></param>
        public PartialDownloader(string url, string dir, string fileGuid, int from, int to, bool rangeAllowed)
        {
            _from = from;
            _to = to;
            _url = url;
            _rangeAllowed = rangeAllowed;
            _fileGuid = fileGuid;
            _directory = dir;
            _lastSpeeds = new int[10];
            _stp = new Stopwatch();
        }

        #endregion

        void DownloadProcedure()
        {
            _file = new FileStream(FullPath, FileMode.Create, FileAccess.ReadWrite);

            #region Request-Response

            _req = WebRequest.Create(_url) as HttpWebRequest;
            if (_req != null)
            {
                _req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                _req.AllowAutoRedirect = true;
                _req.MaximumAutomaticRedirections = 5;
                _req.ServicePoint.ConnectionLimit += 1;
                _req.ServicePoint.Expect100Continue = true;
                _req.ProtocolVersion = HttpVersion.Version10;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                ServicePointManager.Expect100Continue = true;
                if (_rangeAllowed)
                    _req.AddRange(_from, _to);
                _resp = _req.GetResponse() as HttpWebResponse;

                #endregion

                #region Some Stuff

                if (_resp != null)
                {
                    _contentLength = _resp.ContentLength;
                    if (_contentLength <= 0 || (_rangeAllowed && _contentLength != _to - _from + 1))
                        throw new Exception("Invalid response content");
                    _tempStream = _resp.GetResponseStream();
                    int bytesRead;
                    byte[] buffer = new byte[4096];
                    _stp.Start();

                    #endregion

                    #region Procedure Loop

                    while (_tempStream != null && (bytesRead = _tempStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        while (_wait)
                        {
                        }

                        if (_totalBytesRead + bytesRead > _contentLength)
                            bytesRead = (int)(_contentLength - _totalBytesRead);
                        _file.Write(buffer, 0, bytesRead);
                        _totalBytesRead += bytesRead;
                        _lastSpeeds[_counter] = (int)(_totalBytesRead / Math.Ceiling(_stp.Elapsed.TotalSeconds));
                        _counter = (_counter >= 9) ? 0 : _counter + 1;
                        int tempProgress = (int)(_totalBytesRead * 100 / _contentLength);
                        if (_progress != tempProgress)
                        {
                            _progress = tempProgress;
                            _aop.Post(state =>
                            {
                                DownloadPartProgressChanged?.Invoke(this, EventArgs.Empty);
                            }, null);
                        }

                        if (_stop || (_rangeAllowed && _totalBytesRead == _contentLength))
                        {
                            break;
                        }
                    }

                    #endregion

                    #region Close Resources

                    _file.Close();
                    _resp.Close();
                }

                _tempStream?.Close();
                _req.Abort();
            }

            _stp.Stop();

            #endregion

            #region Fire Events

            if (!_stop && DownloadPartCompleted != null)
                _aop.Post(state =>
                {
                    _completed = true;
                    DownloadPartCompleted(this, EventArgs.Empty);
                }, null);

            if (_stop && DownloadPartStopped != null)
                _aop.Post(state => DownloadPartStopped(this, EventArgs.Empty), null);

            #endregion
        }

        #region Public Methods

        /// <summary>
        /// 启动下载
        /// </summary>
        public void Start()
        {
            _stop = false;
            Thread procThread = new Thread(DownloadProcedure);
            procThread.Start();
        }

        /// <summary>
        /// 下载停止
        /// </summary>
        public void Stop()
        {
            _stop = true;
        }

        /// <summary>
        /// 暂停等待下载
        /// </summary>
        public void Wait()
        {
            _wait = true;
        }

        /// <summary>
        /// 稍后唤醒
        /// </summary>
        public void ResumeAfterWait()
        {
            _wait = false;
        }

        #endregion

        #region Property Variables

        private readonly int _from;
        private int _to;
        private readonly string _url;
        private readonly bool _rangeAllowed;
        private long _contentLength;
        private int _totalBytesRead;
        private readonly string _fileGuid;
        private readonly string _directory;
        private int _progress;
        private bool _completed;

        #endregion

        #region Properties

        /// <summary>
        /// 下载已停止
        /// </summary>
        public bool Stopped => _stop;

        /// <summary>
        /// 下载已完成
        /// </summary>
        public bool Completed => _completed;

        /// <summary>
        /// 下载进度
        /// </summary>
        public int Progress => _progress;

        /// <summary>
        /// 下载目录
        /// </summary>
        public string Directory => _directory;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName => _fileGuid;

        /// <summary>
        /// 已读字节数
        /// </summary>
        public long TotalBytesRead => _totalBytesRead;

        /// <summary>
        /// 内容长度
        /// </summary>
        public long ContentLength => _contentLength;

        /// <summary>
        /// RangeAllowed
        /// </summary>
        public bool RangeAllowed => _rangeAllowed;

        /// <summary>
        /// url
        /// </summary>
        public string Url => _url;

        /// <summary>
        /// to
        /// </summary>
        public int To
        {
            get => _to;
            set
            {
                _to = value;
                _contentLength = _to - _from + 1;
            }
        }

        /// <summary>
        /// from
        /// </summary>
        public int From => _from;

        /// <summary>
        /// 当前位置
        /// </summary>
        public int CurrentPosition => _from + _totalBytesRead - 1;

        /// <summary>
        /// 剩余字节数
        /// </summary>
        public int RemainingBytes => (int)(_contentLength - _totalBytesRead);

        /// <summary>
        /// 完整路径
        /// </summary>
        public string FullPath => Path.Combine(_directory, _fileGuid);

        /// <summary>
        /// 下载速度
        /// </summary>
        public int SpeedInBytes
        {
            get
            {
                if (_completed)
                    return 0;

                int totalSpeeds = _lastSpeeds.Sum();

                return totalSpeeds / 10;
            }
        }

        #endregion
    }
}