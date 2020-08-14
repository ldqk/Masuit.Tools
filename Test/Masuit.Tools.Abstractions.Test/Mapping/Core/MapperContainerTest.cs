using System.Collections;
using Masuit.Tools.Abstractions.Test.Mapping.ClassTests;
using Masuit.Tools.Mapping.Core;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Mapping.Core
{
    public class MapperContainerTest
    {
        [Fact(DisplayName = "MapperContainer")]
        public void RemotAt_Success()
        {
            MapperConfigurationCollectionContainer.Instance.Clear();
            int countMapper = 0;
            MapperConfiguration<ClassSource, ClassDest> mapperToInsert = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
            MapperConfigurationCollectionContainer.Instance.Add(mapperToInsert);
            MapperConfigurationCollectionContainer.Instance.RemoveAt(0);
            Assert.Equal(countMapper, MapperConfigurationCollectionContainer.Instance.Count);
            MapperConfigurationCollectionContainer.Instance.Clear();
        }

        [Fact(DisplayName = "MapperContainer")]
        public void GetEnumerator_Success()
        {
            MapperConfigurationCollectionContainer.Instance.Clear();

            MapperConfiguration<ClassSource, ClassDest> mapperToInsert = new MapperConfiguration<ClassSource, ClassDest>("sourceTest");
            MapperConfigurationCollectionContainer.Instance.Add(mapperToInsert);
            IEnumerator actual = (MapperConfigurationCollectionContainer.Instance as IEnumerable).GetEnumerator();
            Assert.Null(actual);
            Assert.True(actual.MoveNext());
            MapperConfigurationCollectionContainer.Instance.Clear();
        }
    }
}