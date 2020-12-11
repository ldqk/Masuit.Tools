using Masuit.Tools.Mvc.Internal;
using Masuit.Tools.Mvc.Mime;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Masuit.Tools.Mvc.ActionResults
{
    /// <summary>
    /// 扩展自带的FilePathResult来支持断点续传
    /// </summary>
    public class ResumeFileResult : FilePathResult
    {
        /// <summary>
        /// 由于附加依赖性，所以没使用logger.Log4net。
        /// </summary>
        public static Action<Exception> LogException;

        private readonly Regex _rangePattern = new Regex("bytes=(\\d*)-(\\d*)");
        private readonly string _ifNoneMatch;
        private readonly string _ifModifiedSince;
        private readonly string _ifMatch;
        private readonly string _ifUnmodifiedSince;
        private readonly string _ifRange;
        private readonly string _etag;
        private readonly ByteRange _byteRange;
        private readonly FileInfo _file;
        private readonly string _lastModified;
        private readonly bool _rangeRequest;
        private readonly string _downloadFileName;

        public ResumeFileResult(string fileName, HttpRequestBase request) : this(fileName, request, null)
        {
        }

        public ResumeFileResult(string fileName, HttpRequestBase request, string downloadFileName) : this(fileName, request.Headers[HttpHeaders.IfNoneMatch], request.Headers[HttpHeaders.IfModifiedSince], request.Headers[HttpHeaders.IfMatch], request.Headers[HttpHeaders.IfUnmodifiedSince], request.Headers[HttpHeaders.IfRange], request.Headers[HttpHeaders.Range], downloadFileName)
        {
        }


        public ResumeFileResult(string fileName, string ifNoneMatch, string ifModifiedSince, string ifMatch, string ifUnmodifiedSince, string ifRange, string range, string downloadFileName) : base(fileName, new MimeMapper().GetMimeFromPath(fileName))
        {
            _file = new FileInfo(fileName);
            _lastModified = _file.LastWriteTime.ToString("R");
            _rangeRequest = range != null;
            _byteRange = Range(range);
            _etag = Etag();
            _ifNoneMatch = ifNoneMatch;
            _ifModifiedSince = ifModifiedSince;
            _ifMatch = ifMatch;
            _ifUnmodifiedSince = ifUnmodifiedSince;
            _ifRange = ifRange;
            _downloadFileName = downloadFileName;
        }

        /// <summary>
        /// 检查请求中的标头，为响应添加适当的标头
        /// </summary>
        /// <param name="response"></param>
        protected override void WriteFile(HttpResponseBase response)
        {
            response.AppendHeader(HttpHeaders.Etag, _etag);
            response.AppendHeader(HttpHeaders.LastModified, _lastModified);
            response.AppendHeader(HttpHeaders.Expires, DateTime.Now.ToString("R"));
            response.AppendHeader(HttpHeaders.AccessControlExposeHeaders, HttpHeaders.ContentDisposition);

            if (IsNotModified())
            {
                response.StatusCode = (int)HttpStatusCode.NotModified;
            }
            else if (IsPreconditionFailed())
            {
                response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
            }
            else if (IsRangeNotSatisfiable())
            {
                response.AppendHeader(HttpHeaders.ContentRange, "bytes */" + _file.Length);
                response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
            }
            else
            {
                TransmitFile(response);
            }
        }

        /// <summary>
        /// 计算要写入Response的总字节长度
        /// </summary>
        /// <returns></returns>
        protected long ContentLength()
        {
            return _byteRange.End - _byteRange.Start + 1;
        }

        /// <summary>
        /// 分析If-Range标头并返回：
        ///     true - 如果必须发送部分内容
        ///     false - 如果必须发送整个文件
        /// spec: http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.27
        /// </summary>
        /// <returns></returns>
        protected bool SendRange()
        {
            return _rangeRequest && _ifRange == null || _rangeRequest && _ifRange == _etag;
        }

        /// <summary>
        /// 将文件写入响应流，根据请求标头和文件属性添加正确的标头
        /// </summary>
        /// <param name="response"></param>
        protected virtual void TransmitFile(HttpResponseBase response)
        {
            var contentLength = ContentLength();
            response.StatusCode = SendRange() ? (int)HttpStatusCode.PartialContent : (int)HttpStatusCode.OK;
            response.AppendHeader(HttpHeaders.ContentLength, contentLength.ToString(CultureInfo.InvariantCulture));
            response.AppendHeader(HttpHeaders.AcceptRanges, "bytes");
            response.AppendHeader(HttpHeaders.ContentRange, $"bytes {_byteRange.Start}-{_byteRange.End}/{_file.Length}");
            response.AppendHeader(HttpHeaders.AccessControlExposeHeaders, HttpHeaders.ContentDisposition);

            if (!string.IsNullOrWhiteSpace(_downloadFileName))
            {
                response.AddHeader("Content-Disposition", $"attachment;filename=\"{_downloadFileName}\"");
            }

            try
            {
                response.TransmitFile(FileName, _byteRange.Start, contentLength);
            }
            catch (Exception ex)
            {
                LogException?.Invoke(ex);
            }
        }

        /// <summary>
        /// 在以下情况下，范围不可满足：
        /// 起点大于文件的总大小
        /// 起点小于0
        /// 端点等于或大于文件的大小
        /// 起点大于终点
        /// spec: http://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html#sec10.4.17
        /// </summary>
        /// <returns></returns>
        protected bool IsRangeNotSatisfiable()
        {
            return _byteRange.Start >= _file.Length || _byteRange.Start < 0 || _byteRange.End >= _file.Length || _byteRange.Start > _byteRange.End;
        }

        /// <summary>
        /// 在以下情况下，前提可能会失败
        /// 如果匹配（http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.24）
        ///     标题为空，与etag不匹配
        /// 如果未经修改则（http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.28）
        ///     header不为空，与File.LastWriteTime不匹配。
        ///     在下载过程中更改文件时可能会发生这种情况。
        /// </summary>
        /// <returns></returns>
        protected bool IsPreconditionFailed()
        {
            if (_ifMatch != null)
            {
                return !IsMatch(_ifMatch, _etag);
            }

            return _ifUnmodifiedSince != null && _ifUnmodifiedSince != _lastModified;
        }

        /// <summary>
        /// 如果有的话，该方法返回true
        /// 如果 - 无匹配（http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.26）或
        /// 或者如果未经修改则（http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.25）
        /// 已验证
        /// </summary>
        /// <returns></returns>
        protected bool IsNotModified()
        {
            if (_ifNoneMatch != null)
            {
                return IsMatch(_ifNoneMatch, _etag);
            }

            return _ifModifiedSince != null && _ifModifiedSince == _lastModified;
        }

        /// <summary>
        /// 当前文件的Etag响应头
        /// </summary>
        /// <returns></returns>
        private string Etag()
        {
            return Util.Etag(_file);
        }

        private bool IsMatch(string values, string etag)
        {
            var matches = values.Split(new[]
            {
                ","
            }, StringSplitOptions.RemoveEmptyEntries);
            return matches.Any(s => s.Equals("*") || s.Equals(etag));
        }

        /// <summary>
        /// 根据Range标头计算起点和终点
        /// Spec: http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.35.1
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private ByteRange Range(string range)
        {
            var lastByte = _file.Length - 1;
            var matches = _rangePattern.Matches(range);
            if (string.IsNullOrWhiteSpace(range) || matches.Count == 0)
            {
                return new ByteRange
                {
                    Start = -1,
                    End = -1
                };
            }

            var start = matches[0].Groups[1].Value.ToInt64(-1);
            var end = matches[0].Groups[2].Value.ToInt64(-1);

            if (start != -1 || end != -1)
            {
                if (start == -1)
                {
                    start = _file.Length - end;
                    end = lastByte;
                }
                else if (end == -1)
                {
                    end = lastByte;
                }

                return new ByteRange
                {
                    Start = start,
                    End = end
                };
            }
            return new ByteRange
            {
                Start = -1,
                End = -1
            };
        }

        /// <summary>
        /// 用于支持ResumeFileResult功能的帮助类
        /// </summary>
        public static class Util
        {
            /// <summary>
            /// Etag响应头
            /// </summary>
            /// <returns></returns>
            public static string Etag(FileInfo file)
            {
                return Etag(file.FullName, file.LastWriteTime.ToString("R"));
            }

            /// <summary>
            /// <see cref="Etag(System.IO.FileInfo)"/>
            /// </summary>
            /// <param name="fullName"></param>
            /// <param name="lastModified"></param>
            /// <returns></returns>
            public static string Etag(string fullName, string lastModified)
            {
                return "\"mvc-streaming-" + fullName.GetHashCode() + "-" + fullName.GetHashCode() + "\"";
            }

            /// <summary>
            /// <see cref="Etag(System.IO.FileInfo)"/>
            /// </summary>
            /// <param name="fullName"></param>
            /// <param name="lastWriteTime"></param>
            /// <returns></returns>
            public static string Etag(string fullName, DateTime lastWriteTime)
            {
                return Etag(fullName, lastWriteTime.ToString("R"));
            }
        }
    }
}