using Masuit.Tools.Mvc.Mime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Masuit.Tools.Mvc.ActionResults
{
    public abstract class ResumeActionResultBase : ActionResult
    {
        [DefaultValue("<q1w2e3r4t5y6u7i8o9p0>")]
        public string MultipartBoundary { get; set; }

        public string ContentType { get; }
        private readonly string _fileName;
        public DateTimeOffset? LastModified { get; set; }
        public string EntityTag { get; set; }
        public Stream FileContents { get; set; }

        protected ResumeActionResultBase(string fileName)
        {
            var mimeMapper = new MimeMapper();
            string contentType = mimeMapper.GetMimeFromPath(fileName);
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = MimeMapper.DefaultMime;
            }
            _fileName = fileName;
            ContentType = contentType;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.HttpContext.Response.Headers[HttpHeaders.AccessControlExposeHeaders] = HttpHeaders.ContentDisposition;
            ExecuteResultBody(context, new ResumeRequest(context.HttpContext, FileContents.Length)
            {
                FileName = _fileName
            });
        }

        public virtual void ExecuteResultBody(ControllerContext context, ResumeRequest resumingRequest)
        {
            WriteCommonHeaders(context, resumingRequest);

            if (ShouldProceedAfterEvaluatingPreconditions(context.HttpContext, resumingRequest))
            {
                using (FileContents)
                {
                    if (resumingRequest.IsRangeRequest)
                    {
                        WritePartialContent(context, FileContents, resumingRequest);
                    }
                    else
                    {
                        WriteFullContent(context, FileContents);
                    }
                }
            }
        }


        protected virtual bool ShouldProceedAfterEvaluatingPreconditions(HttpContextBase context, ResumeRequest resumingRequest)
        {
            var request = context.Request;
            string check;
            DateTimeOffset preconditionDateTime;

            if (!string.IsNullOrEmpty(check = (request.Headers[HttpWorkerRequest.GetKnownRequestHeaderName(HttpWorkerRequest.HeaderIfRange)])))
            {
                if (DateTimeOffset.TryParse(check, out preconditionDateTime))
                {
                    if ((LastModified.Value - preconditionDateTime).TotalSeconds > 1)
                    {
                        resumingRequest.Ranges = null;
                    }
                }
                else
                {
                    if (!check.Equals(EntityTag))
                    {
                        resumingRequest.Ranges = null;
                    }
                }
            }


            if (!string.IsNullOrEmpty(check = (request.Headers[HttpWorkerRequest.GetKnownRequestHeaderName(HttpWorkerRequest.HeaderIfMatch)])))
            {
                IEnumerable<string> entitiesTags = check.Split(',');

                if ((string.IsNullOrEmpty(EntityTag) && entitiesTags.Any()) || (!entitiesTags.Any(entity => entitiesTags.Equals(EntityTag))))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(check = (request.Headers[HttpWorkerRequest.GetKnownRequestHeaderName(HttpWorkerRequest.HeaderIfNoneMatch)])))
            {
                IEnumerable<string> entitiesTag = check.Split(',');
                if ((!string.IsNullOrEmpty(EntityTag) && entitiesTag.Contains("*")) || (entitiesTag.Any(entity => entity.Equals(EntityTag))))
                {
                    if (context.Request.RequestType == "GET" || context.Request.RequestType == "HEAD")
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                        return false;
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(check = (request.Headers[HttpWorkerRequest.GetKnownRequestHeaderName(HttpWorkerRequest.HeaderIfUnmodifiedSince)])))
            {
                if (DateTimeOffset.TryParse(check, out preconditionDateTime))
                {
                    if (!LastModified.HasValue || ((LastModified.Value - preconditionDateTime).TotalSeconds > 0))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
                        return false;
                    }
                }
            }


            if (!string.IsNullOrEmpty(check = (request.Headers[HttpWorkerRequest.GetKnownRequestHeaderName(HttpWorkerRequest.HeaderIfModifiedSince)])))
            {
                if (DateTimeOffset.TryParse(check, out preconditionDateTime))
                {
                    if (LastModified.HasValue)
                    {
                        if ((LastModified.Value - preconditionDateTime).TotalSeconds < 1)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        protected virtual void WriteCommonHeaders(ControllerContext context, ResumeRequest resumingRequest)
        {
            context.HttpContext.Response.ContentType = resumingRequest.IsMultipartRequest ? $"multipart/byteranges; boundary={MultipartBoundary}" : ContentType;

            context.HttpContext.Response.AddHeader(HttpWorkerRequest.GetKnownResponseHeaderName(HttpWorkerRequest.HeaderAcceptRanges), "bytes");

            if (!string.IsNullOrEmpty(resumingRequest.FileName))
            {
                context.HttpContext.Response.AddHeader("Content-Disposition", $"inline; filename=\"{resumingRequest.FileName}\"");
            }

            if (!string.IsNullOrEmpty(EntityTag))
            {
                context.HttpContext.Response.AddHeader(HttpWorkerRequest.GetKnownResponseHeaderName(HttpWorkerRequest.HeaderEtag), EntityTag);
            }

            if (LastModified.HasValue)
            {
                context.HttpContext.Response.AddHeader(HttpWorkerRequest.GetKnownResponseHeaderName(HttpWorkerRequest.HeaderLastModified), LastModified.Value.ToString("R"));
            }
        }

        public virtual void WriteFullContent(ControllerContext context, Stream fileContent)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            WriteBinaryData(context, fileContent, 0, fileContent.Length - 1);
        }

        public virtual void WritePartialContent(ControllerContext context, Stream fileContent, ResumeRequest resumingRequest)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.PartialContent;

            if (!resumingRequest.IsMultipartRequest)
            {
                context.HttpContext.Response.AddHeader(HttpWorkerRequest.GetKnownResponseHeaderName(HttpWorkerRequest.HeaderContentRange), $"bytes {resumingRequest.Ranges.First().Start}-{resumingRequest.Ranges.First().End}/{fileContent.Length}");
            }

            foreach (var range in resumingRequest.Ranges)
            {
                if (!response.IsClientConnected)
                {
                    return;
                }

                if (resumingRequest.IsMultipartRequest)
                {
                    response.Output.WriteLine($"--{MultipartBoundary}");
                    response.Output.WriteLine($"{HttpWorkerRequest.GetKnownResponseHeaderName(HttpWorkerRequest.HeaderContentType)}: {ContentType}");
                    response.Output.WriteLine($"{HttpWorkerRequest.GetKnownResponseHeaderName(HttpWorkerRequest.HeaderContentRange)}: bytes {resumingRequest.Ranges.First().Start}-{resumingRequest.Ranges.First().End}/{fileContent.Length}");
                    response.Output.WriteLine();
                }

                WriteBinaryData(context, fileContent, range.Start, range.End);

                if (resumingRequest.IsMultipartRequest)
                {
                    response.Output.WriteLine();
                }
            }

            if (resumingRequest.IsMultipartRequest)
            {
                response.Output.WriteLine($"--{MultipartBoundary}--");
                response.Output.WriteLine();
            }
        }

        private void WriteBinaryData(ControllerContext context, Stream fileContent, long startIndex, long endIndex)
        {
            var response = context.HttpContext.Response;
            response.BufferOutput = false;
            byte[] buffer = new byte[0x1000];
            long totalToSend = endIndex - startIndex;
            long bytesRemaining = totalToSend + 1;
            response.AppendHeader("Content-Length", bytesRemaining.ToString());
            fileContent.Seek(startIndex, SeekOrigin.Begin);

            while (response.IsClientConnected && bytesRemaining > 0)
            {
                try
                {
                    var count = bytesRemaining <= buffer.Length ? fileContent.Read(buffer, 0, (int)bytesRemaining) : fileContent.Read(buffer, 0, buffer.Length);

                    if (count == 0)
                    {
                        return;
                    }

                    response.OutputStream.Write(buffer, 0, count);
                    bytesRemaining -= count;
                }
                catch (IndexOutOfRangeException)
                {
                    response.Output.Flush();
                    return;
                }
                finally
                {
                    response.Output.Flush();
                }
            }
        }
    }
}