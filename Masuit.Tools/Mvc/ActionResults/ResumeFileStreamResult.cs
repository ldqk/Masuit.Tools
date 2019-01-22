using System;
using System.IO;

namespace Masuit.Tools.Mvc.ActionResults
{
    public class ResumeFileStreamResult : ResumeActionResultBase
    {
        public ResumeFileStreamResult(Stream fileStream, string fileName) : base(fileName)
        {
            FileContents = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
        }

        public ResumeFileStreamResult(Stream fileStream, string fileName, string etag) : this(fileStream, fileName)
        {
            EntityTag = etag;
        }
    }
}