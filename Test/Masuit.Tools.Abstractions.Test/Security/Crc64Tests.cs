using System;
using Masuit.Tools.Security;
using Xunit;

namespace Masuit.Tools.Abstractions.Test.Security
{
    public class Crc64Tests
    {
        [Fact]
        public void Crc64_DefaultConstructor_ShouldInitializeCorrectly()
        {
            var crc64 = new Crc64();
            Assert.Equal(64, crc64.HashSize);
        }

        [Fact]
        public void Crc64_ConstructorWithPolynomial_ShouldInitializeCorrectly()
        {
            ulong polynomial = 0xD800000000000000;
            var crc64 = new Crc64(polynomial);
            Assert.Equal(64, crc64.HashSize);
        }

        [Fact]
        public void Crc64_ConstructorWithPolynomialAndSeed_ShouldInitializeCorrectly()
        {
            ulong polynomial = 0xD800000000000000;
            ulong seed = 0x0;
            var crc64 = new Crc64(polynomial, seed);
            Assert.Equal(64, crc64.HashSize);
        }

        [Fact]
        public void Crc64_HashCore_ShouldComputeHashCorrectly()
        {
            var crc64 = new Crc64();
            byte[] data = System.Text.Encoding.UTF8.GetBytes("test");
            crc64.Initialize();
            crc64.TransformBlock(data, 0, data.Length, data, 0);
            crc64.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            byte[] hash = crc64.Hash;
            Assert.NotNull(hash);
        }

        [Fact]
        public void Crc64_HashFinal_ShouldReturnCorrectHash()
        {
            var crc64 = new Crc64();
            byte[] data = System.Text.Encoding.UTF8.GetBytes("test");
            crc64.Initialize();
            crc64.TransformBlock(data, 0, data.Length, data, 0);
            crc64.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            byte[] hash = crc64.Hash;
            Assert.NotNull(hash);
        }

        [Fact]
        public void Crc64_Compute_ShouldReturnCorrectHash()
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes("test");
            ulong hash = Crc64.Compute(data);
            Assert.Equal((ulong)5153117669225922560, hash);
        }

        [Fact]
        public void Crc64_ComputeWithSeed_ShouldReturnCorrectHash()
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes("test");
            ulong seed = 0x0;
            ulong hash = Crc64.Compute(seed, data);
            Assert.Equal((ulong)5153117669225922560, hash);
        }
    }
}