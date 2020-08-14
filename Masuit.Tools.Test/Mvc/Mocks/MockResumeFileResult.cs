using Masuit.Tools.Mvc.ActionResults;
using System.Web;

namespace Masuit.Tools.Test.Mvc.Mocks
{
    public class MockResumeFileResult : ResumeFileResult
    {
        public MockResumeFileResult(string fileName, HttpRequestBase request) : base(fileName, request)
        {
        }

        public MockResumeFileResult(string fileName, HttpRequestBase request, string downloadFileName) : base(fileName, request, downloadFileName)
        {
        }

        public new bool IsNotModified()
        {
            return base.IsNotModified();
        }

        public new bool IsPreconditionFailed()
        {
            return base.IsPreconditionFailed();
        }

        public new bool IsRangeNotSatisfiable()
        {
            return base.IsRangeNotSatisfiable();
        }

        public new bool SendRange()
        {
            return base.SendRange();
        }

        public void WriteFileTest(HttpResponseBase response)
        {
            base.WriteFile(response);
        }

        public void TransmitTest(HttpResponseBase response)
        {
            base.TransmitFile(response);
        }
    }
}