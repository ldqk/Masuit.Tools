using System.Linq;
using Masuit.Tools.Abstractions.Hardware;
using Masuit.Tools.Hardware;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Hardware
{
    public class Aida64Tests
    {
        [Fact]
        public void GetValuesFromSharedMemTest()
        {
            var values = SystemInfo.GetAida64Values();

            Assert.True(values.Any());
            Assert.False(values.Any(e => e.Value == null));
            Assert.False(values.Any(e => e.Name == null));
            Assert.False(values.Any(e => e.Identifier == null));
            Assert.False(values.Any(e => e.Type == SensorTypes.Unknown));
        }
    }
}