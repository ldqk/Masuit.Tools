using System;
using System.IO;

namespace Masuit.Tools.Mvc.ActionResults
{
    public class ResumeFileContentResult : ResumeActionResultBase
    {
        public ResumeFileContentResult(byte[] fileContents, string fileName) : base(fileName)
        {
            if (fileContents == null)
            {
                throw new ArgumentNullException(nameof(fileContents));
            }

            FileContents = new MemoryStream(fileContents);
        }

        public ResumeFileContentResult(byte[] fileContents, string fileName, string etag) : this(fileContents, fileName)
        {
            EntityTag = etag;
        }
    }
}