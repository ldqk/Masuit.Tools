using Masuit.Tools.Mvc.Internal;
using System.Web;

namespace Masuit.Tools.Mvc.ActionResults
{
    public class ResumeRequest
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public ByteRange[] Ranges { get; set; }

        public bool IsRangeRequest => (Ranges != null && Ranges.Length > 0);

        public bool IsMultipartRequest => (Ranges != null && Ranges.Length > 1);

        public ResumeRequest(HttpContextBase context, long contentLength)
        {
            var request = context.Request;
            ContentType = request.ContentType;

            if (!string.IsNullOrEmpty(request.FilePath))
            {
                FileName = VirtualPathUtility.GetFileName(request.FilePath);
            }

            ParseRequestHeaderRanges(context, contentLength);
        }


        protected virtual void ParseRequestHeaderRanges(HttpContextBase context, long contentSize)
        {
            var request = context.Request;
            string rangeHeader = request.Headers[HttpWorkerRequest.GetKnownRequestHeaderName(HttpWorkerRequest.HeaderRange)];

            if (string.IsNullOrEmpty(rangeHeader))
            {
                return;
            }

            string[] ranges = rangeHeader.Replace("bytes=", string.Empty).Split(",".ToCharArray());
            Ranges = new ByteRange[ranges.Length];
            for (int i = 0; i < ranges.Length; i++)
            {
                const int start = 0, end = 1;
                string[] currentRange = ranges[i].Split("-".ToCharArray());

                if (long.TryParse(currentRange[end], out var parsedValue))
                {
                    Ranges[i].End = parsedValue;
                }
                else
                {
                    Ranges[i].End = contentSize - 1;
                }

                if (long.TryParse(currentRange[start], out parsedValue))
                {
                    Ranges[i].Start = parsedValue;
                }
                else
                {
                    Ranges[i].Start = contentSize - Ranges[i].End;
                    Ranges[i].End = contentSize - 1;
                }
            }
        }
    }
}