using Mvc.Stream.Tests.Mocks;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Masuit.Tools.Test.Mvc.Mocks
{
    public class MockHttpFilesCollection : HttpFileCollectionBase
    {
        private readonly Dictionary<string, MockHttpPostedFileBase> _files
            = new Dictionary<string, MockHttpPostedFileBase>();

        public MockHttpFilesCollection(MockHttpPostedFileBase file)
        {
            if (file != null)
            {
                _files.Add(file.FileName, file);
            }
        }

        public override int Count => _files.Count;

        public override HttpPostedFileBase this[int index] => _files.Skip(index).Take(1).FirstOrDefault().Value;

        public override HttpPostedFileBase this[string name] => _files[name];

        public override string[] AllKeys
        {
            get
            {
                return _files.Select(x => x.Key).ToArray();
            }
        }
    }
}
