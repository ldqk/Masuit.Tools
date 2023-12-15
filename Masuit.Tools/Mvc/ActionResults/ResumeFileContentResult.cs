using System;
using System.IO;
using Masuit.Tools.Systems;

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

            FileContents = new PooledMemoryStream(fileContents);
        }

        public ResumeFileContentResult(byte[] fileContents, string fileName, string etag) : this(fileContents, fileName)
        {
            EntityTag = etag;
        }
    }
}
