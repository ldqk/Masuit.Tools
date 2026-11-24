using System;
using Masuit.Tools.Core.AspNetCore;
using Masuit.Tools.Files;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Masuit.Tools.Core.Test.AspNetCore
{
    public class SevenZipRegistrationTests
    {
        [Fact]
        public void AddSevenZipCompressor_RegistersBothInterfaces()
        {
            using var provider = new ServiceCollection().AddSevenZipCompressor().BuildServiceProvider();
            Assert.IsType<SevenZipCompressor>(provider.GetRequiredService<ISevenZipCompressor>());
            var compressor = provider.GetRequiredService<ISevenZipCompressor>();
            using var archive = compressor.SevenZipStream(Array.Empty<string>());
            Assert.True(archive.Length > 6);
        }
    }
}
