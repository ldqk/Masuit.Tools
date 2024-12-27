using Masuit.Tools.Mvc;
using Masuit.Tools.Mvc.ActionResults;
using Masuit.Tools.Test.Mvc.Mocks;
using System;
using System.IO;
using System.Net;
using System.Threading;
using Xunit;

namespace Masuit.Tools.Test.Mvc
{
    public class ResumeFileResultTests : BaseTests
    {
        private FileInfo _file;
        private FileInfo _file2;

        public ResumeFileResultTests()
        {
            _file = new FileInfo(FilePath("download-test-file.txt"));
            _file2 = new FileInfo(FilePath("download-test-file2.txt"));
            Request.Headers.Clear();
            Response.ClearTestResponse();
        }

        [Fact]
        public void CanCalculateEtagForFile()
        {
            Assert.NotNull(ResumeFileResult.Util.Etag(_file));
        }

        [Fact]
        public void EtagDoesNotDependOnTime()
        {
            var etag1 = ResumeFileResult.Util.Etag(_file);
            Thread.Sleep(100);
            var etag2 = ResumeFileResult.Util.Etag(_file);
            Assert.Equal(etag1, etag2);
        }

        [Fact]
        public void EtagDoesDependOnFile()
        {
            var etag1 = ResumeFileResult.Util.Etag(_file);
            Thread.Sleep(100);
            var etag2 = ResumeFileResult.Util.Etag(_file2);
            Assert.NotEqual(etag1, etag2);
        }

        [Fact]
        public void IsRangeNotSatisfiable_Is_True_If_Range_Header_Has_Invalid_Format()
        {
            Request.Headers[HttpHeaders.Range] = "blah";
            Assert.True(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Fact]
        public void IsRangeNotSatisfiable_Is_True_If_Start_Greater_Than_End()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=100-0";
            Assert.True(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Fact]
        public void IsRangeNotSatisfiable_Is_True_If_End_Equals_Total_File_Size()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-" + _file.Length;
            Assert.True(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Fact]
        public void IsRangeNotSatisfiable_Is_True_If_End_Greater_Than_Total_File_Size()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-" + _file.Length + 10;
            Assert.True(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Fact]
        public void IsRangeNotSatisfiable_Is_False_If_Range_Has_StartsWith_Format()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-";
            Assert.False(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Fact]
        public void IsRangeNotSatisfiable_Is_False_If_Range_Has_LastXbytes_Format()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=-100";
            Assert.False(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Fact]
        public void IsRangeNotSatisfiable_Is_False_If_Range_Ends_With_Last_Byte_Position()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=100-" + (_file.Length - 1);
            Assert.False(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Fact]
        public void SendRange_Is_True_If_Range_Is_Correct_And_ifRange_Is_Null()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-100";
            Assert.True(new MockResumeFileResult(_file.FullName, Request).SendRange());
        }

        [Fact]
        public void SendRange_Is_True_If_Range_And_ifRange_Are_Correct()
        {
            Request.Headers[HttpHeaders.IfRange] = ResumeFileResult.Util.Etag(_file);
            Request.Headers[HttpHeaders.Range] = "bytes=0-100";
            Assert.True(new MockResumeFileResult(_file.FullName, Request).SendRange());
        }

        [Fact]
        public void SendRange_Is_False_If_Range_Is_Correct_But_ifRange_Is_InCorrect()
        {
            Request.Headers[HttpHeaders.IfRange] = "incorrect etag";
            Request.Headers[HttpHeaders.Range] = "bytes=0-100";
            Assert.False(new MockResumeFileResult(_file.FullName, Request).SendRange());
        }

        [Fact]
        public void HeadersTest_Should_Not_Send_File_Is_RangeNotSatisfiable()
        {
            Request.Headers[HttpHeaders.Range] = "invalid";
            var result = new MockResumeFileResult(_file.FullName, Request);
            Assert.True(result.IsRangeNotSatisfiable());
            result.WriteFileTest(Response);
            Assert.Equal((int)HttpStatusCode.RequestedRangeNotSatisfiable, Response.StatusCode);
            Assert.NotNull(Response.Headers[HttpHeaders.Etag]);
            Assert.NotNull(Response.Headers[HttpHeaders.Expires]);
            Assert.NotNull(Response.Headers[HttpHeaders.LastModified]);
            Assert.Equal("bytes */" + _file.Length, Response.Headers[HttpHeaders.ContentRange]);
            Assert.Equal(0, Response.OutputStream.Length);
        }

        [Fact]
        public void Range_First_500b()
        {
            var stream = GetResponseStream("bytes=0-499");
            Assert.Equal(500, stream.Length);
            Assert.Equal(206, Response.StatusCode);
            Assert.Equal($"bytes 0-499/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Fact]
        public void Range_From_500b_to_899b()
        {
            var stream = GetResponseStream("bytes=500-899");
            Assert.Equal(400, stream.Length);
            Assert.Equal(206, Response.StatusCode);
            Assert.Equal($"bytes 500-899/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Fact]
        public void Range_Last_300b()
        {
            var stream = GetResponseStream("bytes=-300");
            Assert.Equal(300, stream.Length);
            Assert.Equal(206, Response.StatusCode);
            var from = _file.Length - 300;
            var to = _file.Length - 1;

            Assert.Equal($"bytes {from}-{to}/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Fact]
        public void Range_From_100b_toThe_End()
        {
            var stream = GetResponseStream($"bytes={(_file.Length - 100)}-");
            Assert.Equal(100, stream.Length);
            Assert.Equal(206, Response.StatusCode);
            var from = _file.Length - 100;
            var to = _file.Length - 1;
            Assert.Equal($"bytes {from}-{to}/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Fact]
        public void Range_First_1b()
        {
            var stream = GetResponseStream("bytes=0-0");
            Assert.Equal(1, stream.Length);
            Assert.Equal(206, Response.StatusCode);
            Assert.Equal($"bytes 0-0/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Fact]
        public void Range_Last_1b()
        {
            var stream = GetResponseStream("bytes=-1");
            Assert.Equal(1, stream.Length);
            Assert.Equal(206, Response.StatusCode);
            var from = _file.Length - 1;
            var to = _file.Length - 1;
            Assert.Equal($"bytes {from}-{to}/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Fact]
        public void Range_Whole_File_With_RangeHeader()
        {
            var stream = GetResponseStream("bytes=0-" + (_file.Length - 1));
            Assert.Equal(_file.Length, stream.Length);
            Assert.Equal(206, Response.StatusCode);
            Assert.Equal($"bytes 0-{(_file.Length - 1)}/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Fact]
        public void TransmissionRange_From_0_To_0()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-0";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            Assert.Equal(1, Response.OutputStream.Length);
            AssertBytes(_file, Response.OutputStream, 0, 1);
        }

        [Fact]
        public void TransmissionRange_From_1_To_100()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=1-100";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            Assert.Equal(100, Response.OutputStream.Length);
            AssertBytes(_file, Response.OutputStream, 1, 100);
        }

        [Fact]
        public void TransmissionRange_From_101_To_theEnd()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=101-";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            Assert.Equal(_file.Length - 101, Response.OutputStream.Length);
            AssertBytes(_file, Response.OutputStream, 101, (int)_file.Length);
        }

        [Fact]
        public void TransmissionRange_WholeFile_WithRangeHeader()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            Assert.Equal(_file.Length, Response.OutputStream.Length);
            AssertBytes(_file, Response.OutputStream, 0, (int)_file.Length);
        }

        [Fact]
        public void ShouldSend206If_Range_HeaderExists()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);
            Assert.Equal(206, Response.StatusCode);
        }

        [Fact]
        public void Etag_Should_Be_Added_To_Response_If_It_Equals_With_IfRange_In_Request__PartialResponse()
        {
            var etag = ResumeFileResult.Util.Etag(_file);
            Request.Headers[HttpHeaders.IfRange] = etag;
            Request.Headers[HttpHeaders.Range] = "bytes=0-";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);
            Assert.Equal(Response.Headers[HttpHeaders.Etag], etag);
            Assert.NotNull(Response.Headers[HttpHeaders.ContentRange]);
            Assert.Equal(206, Response.StatusCode);
        }

        private Stream GetResponseStream(string range)
        {
            Response.ClearTestResponse();
            Response.StatusCode = 500;

            Request.Headers[HttpHeaders.Range] = range;
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            return Response.OutputStream;
        }

        private void AssertBytes(FileInfo file, Stream responseStream, int from, int to)
        {
            using (var fileStream = file.OpenRead())
            {
                responseStream.Seek(0, SeekOrigin.Begin);
                fileStream.Seek(from, SeekOrigin.Begin);
                for (var byteIndex = from; byteIndex < to; byteIndex++)
                {
                    var responseByte = responseStream.ReadByte();
                    var fileByte = fileStream.ReadByte();
                    Assert.Equal(responseByte, fileByte);
                }
            }
        }
    }
}