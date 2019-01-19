using Masuit.Tools.Mapping.Core;
using Masuit.Tools.UnitTest.Mapping.ClassTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace Masuit.Tools.UnitTest.Mapping.Core
{
    [TestClass]
    public class MapperContainerTest
    {
        [TestMethod, TestCategory("MapperContainer")]
        public void RemotAt_Success()
        {
            MapperConfigurationCollectionContainer.Instance.Clear();
            var countMapper = 0;
            var mapperToInsert = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
            MapperConfigurationCollectionContainer.Instance.Add(mapperToInsert);
            MapperConfigurationCollectionContainer.Instance.RemoveAt(0);
            Assert.AreEqual(countMapper, MapperConfigurationCollectionContainer.Instance.Count);
            MapperConfigurationCollectionContainer.Instance.Clear();

        }
        [TestMethod, TestCategory("MapperContainer")]
        public void GetEnumerator_Success()
        {
            MapperConfigurationCollectionContainer.Instance.Clear();

            var mapperToInsert = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
            MapperConfigurationCollectionContainer.Instance.Add(mapperToInsert);
            IEnumerator actual = (MapperConfigurationCollectionContainer.Instance as IEnumerable).GetEnumerator();
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.MoveNext());
            MapperConfigurationCollectionContainer.Instance.Clear();

        }
    }
}
