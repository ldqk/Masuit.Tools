using Masuit.Tools.Mvc;
using Masuit.Tools.Mvc.ActionResults;
using Masuit.Tools.Test.Mvc.Mocks;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Masuit.Tools.Test.Mvc
{
    [TestFixture]
    public class ResumeFileResultTests : BaseTests
    {
        private FileInfo _file;
        private FileInfo _file2;

        [SetUp]
        public void Setup()
        {
            _file = new FileInfo(FilePath("download-test-file.txt"));
            _file2 = new FileInfo(FilePath("download-test-file2.txt"));
            Request.Headers.Clear();
            Response.ClearTestResponse();
        }

        [Test]
        public void CanCalculateEtagForFile()
        {
            Assert.IsNotNull(ResumeFileResult.Util.Etag(_file));
        }

        [Test]
        public void EtagDoesNotDependOnTime()
        {
            var etag1 = ResumeFileResult.Util.Etag(_file);
            Thread.Sleep(100);
            var etag2 = ResumeFileResult.Util.Etag(_file);
            Assert.AreEqual(etag1, etag2);
        }

        [Test]
        public void EtagDoesDependOnFile()
        {
            var etag1 = ResumeFileResult.Util.Etag(_file);
            Thread.Sleep(100);
            var etag2 = ResumeFileResult.Util.Etag(_file2);
            Assert.AreNotEqual(etag1, etag2);
        }

        [Test]
        public void IsNotModified_Is_False_If_IfNoneMatch_And_IfModifiedSince_Are_Empty()
        {
            Request.Headers[HttpHeaders.IfNoneMatch] = null;
            Request.Headers[HttpHeaders.IfModifiedSince] = null;
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsNotModified());
        }

        [Test]
        public void IsNotModified_Is_False_If_Etag_Is_Invalid_And_IfModifiedSince_Is_Null()
        {
            var etag = "invalid etag";
            Request.Headers[HttpHeaders.IfNoneMatch] = etag;
            Request.Headers[HttpHeaders.IfModifiedSince] = null;
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsNotModified());
        }

        [Test]
        public void IsNotModified_Is_True_If_Etag_Is_Valid()
        {
            var etag = ResumeFileResult.Util.Etag(_file);
            Request.Headers[HttpHeaders.IfNoneMatch] = etag;
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).IsNotModified());
        }

        [Test]
        public void IsNotModified_Is_True_If_Etag_Is_Star()
        {
            var etag = "*";
            Request.Headers[HttpHeaders.IfNoneMatch] = etag;
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).IsNotModified());
        }

        [Test]
        public void IsNotModified_Is_False_If_Etag_Is_Empty_And_IfModifiedSince_Is_Invalid()
        {
            Request.Headers[HttpHeaders.IfModifiedSince] = DateTime.Now.ToString("R");
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsNotModified());
        }

        [Test]
        public void IsNotModified_Is_False_If_Etag_Is_Empty_And_IfModifiedSince_Is_LastFileWriteTime()
        {
            Request.Headers[HttpHeaders.IfModifiedSince] = _file.LastWriteTime.ToString("R");
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).IsNotModified());
        }

        [Test]
        public void IsPreconditionFailedTest_Is_False_If_ifMatch_And_ifUnmodifiedSince_Are_Empty()
        {
            Request.Headers[HttpHeaders.IfMatch] = null;
            Request.Headers[HttpHeaders.IfUnmodifiedSince] = null;
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsPreconditionFailed());
        }

        [Test]
        public void IsPreconditionFailedTest_Is_IsTrue_If_ifMatch_Doesnot_Match_Etag_Of_The_File()
        {
            Request.Headers[HttpHeaders.IfMatch] = "incorrect";
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).IsPreconditionFailed());
        }

        [Test]
        public void IsPreconditionFailedTest_Is_IsFalse_If_ifMatch_Matches_Etag_Of_The_File()
        {
            Request.Headers[HttpHeaders.IfMatch] = ResumeFileResult.Util.Etag(_file);
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsPreconditionFailed());
        }

        [Test]
        public void IsPreconditionFailedTest_Is_IsFalse_If_ifMatch_Equals_Star()
        {
            Request.Headers[HttpHeaders.IfMatch] = "*";
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsPreconditionFailed());
        }

        [Test]
        public void IsPreconditionFailedTest_Is_IsTrue_If_ifUnmodifiedSince_Doesnot_Equal_FileLastWriteTime()
        {
            Request.Headers[HttpHeaders.IfUnmodifiedSince] = DateTime.Now.ToString("R");
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).IsPreconditionFailed());
        }

        [Test]
        public void IsPreconditionFailedTest_Is_IsFalse_If_ifUnmodifiedSince_Equals_FileLastWriteTime()
        {
            Request.Headers[HttpHeaders.IfUnmodifiedSince] = _file.LastWriteTime.ToString("R");
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsPreconditionFailed());
        }

        [Test]
        public void IsRangeNotSatisfiable_Is_True_If_Range_Header_Has_Invalid_Format()
        {
            Request.Headers[HttpHeaders.Range] = "blah";
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Test]
        public void IsRangeNotSatisfiable_Is_True_If_Start_Greater_Than_End()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=100-0";
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Test]
        public void IsRangeNotSatisfiable_Is_True_If_End_Equals_Total_File_Size()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-" + _file.Length;
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Test]
        public void IsRangeNotSatisfiable_Is_True_If_End_Greater_Than_Total_File_Size()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-" + _file.Length + 10;
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Test]
        public void IsRangeNotSatisfiable_Is_False_If_Range_Header_Is_Null()
        {
            Request.Headers[HttpHeaders.Range] = null;
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Test]
        public void IsRangeNotSatisfiable_Is_False_If_Range_Has_StartsWith_Format()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-";
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Test]
        public void IsRangeNotSatisfiable_Is_False_If_Range_Has_LastXbytes_Format()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=-100";
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Test]
        public void IsRangeNotSatisfiable_Is_False_If_Range_Ends_With_Last_Byte_Position()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=100-" + (_file.Length - 1);
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).IsRangeNotSatisfiable());
        }

        [Test]
        public void SendRange_Is_False_If_Range_And_ifRange_Headers_Are_Null()
        {
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).SendRange());
        }

        [Test]
        public void SendRange_Is_False_If_Range_Is_Null_And_ifRange_Is_Correct()
        {
            Request.Headers[HttpHeaders.IfRange] = ResumeFileResult.Util.Etag(_file);
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).SendRange());
        }

        [Test]
        public void SendRange_Is_True_If_Range_Is_Correct_And_ifRange_Is_Null()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-100";
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).SendRange());
        }

        [Test]
        public void SendRange_Is_True_If_Range_And_ifRange_Are_Correct()
        {
            Request.Headers[HttpHeaders.IfRange] = ResumeFileResult.Util.Etag(_file);
            Request.Headers[HttpHeaders.Range] = "bytes=0-100";
            Assert.IsTrue(new MockResumeFileResult(_file.FullName, Request).SendRange());
        }

        [Test]
        public void SendRange_Is_False_If_Range_Is_Correct_But_ifRange_Is_InCorrect()
        {
            Request.Headers[HttpHeaders.IfRange] = "incorrect etag";
            Request.Headers[HttpHeaders.Range] = "bytes=0-100";
            Assert.IsFalse(new MockResumeFileResult(_file.FullName, Request).SendRange());
        }

        [Test]
        public void HeadersTest_Should_Not_Send_File_If_File_Has_Not_Been_Changed()
        {
            Request.Headers[HttpHeaders.IfNoneMatch] = ResumeFileResult.Util.Etag(_file);
            var result = new MockResumeFileResult(_file.FullName, Request);
            Assert.IsTrue(result.IsNotModified());
            result.WriteFileTest(Response);
            Assert.AreEqual((int)HttpStatusCode.NotModified, Response.StatusCode);
            Assert.IsNotNull(Response.Headers[HttpHeaders.Etag]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.Expires]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.LastModified]);
            Assert.IsNull(Response.Headers[HttpHeaders.ContentRange]);
            Assert.AreEqual(0, Response.OutputStream.Length);
        }

        [Test]
        public void HeadersTest_Should_Not_Send_File_IfPreconditionFailed()
        {
            Request.Headers[HttpHeaders.IfMatch] = "invalid";
            var result = new MockResumeFileResult(_file.FullName, Request);
            Assert.IsTrue(result.IsPreconditionFailed());

            result.WriteFileTest(Response);
            Assert.AreEqual((int)HttpStatusCode.PreconditionFailed, Response.StatusCode);
            Assert.IsNotNull(Response.Headers[HttpHeaders.Etag]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.Expires]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.LastModified]);
            Assert.IsNull(Response.Headers[HttpHeaders.ContentRange]);
            Assert.AreEqual(0, Response.OutputStream.Length);
        }

        [Test]
        public void HeadersTest_Should_Not_Send_File_Is_RangeNotSatisfiable()
        {
            Request.Headers[HttpHeaders.Range] = "invalid";
            var result = new MockResumeFileResult(_file.FullName, Request);
            Assert.IsTrue(result.IsRangeNotSatisfiable());
            result.WriteFileTest(Response);
            Assert.AreEqual((int)HttpStatusCode.RequestedRangeNotSatisfiable, Response.StatusCode);
            Assert.IsNotNull(Response.Headers[HttpHeaders.Etag]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.Expires]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.LastModified]);
            Assert.AreEqual("bytes */" + _file.Length, Response.Headers[HttpHeaders.ContentRange]);
            Assert.AreEqual(0, Response.OutputStream.Length);
        }

        [Test]
        public void HeadersTest_Should_Send_File_If_All_Headers_Are_Null()
        {
            var result = new MockResumeFileResult(_file.FullName, Request);
            result.WriteFileTest(Response);
            Assert.AreEqual((int)HttpStatusCode.OK, Response.StatusCode);
            Assert.IsNotNull(Response.Headers[HttpHeaders.Etag]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.Expires]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.LastModified]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.ContentRange]);
            Assert.IsNotNull(Response.Headers[HttpHeaders.ContentLength]);
            Assert.AreEqual(_file.Length, Response.OutputStream.Length);
        }

        [Test]
        public void Range_First_500b()
        {
            var stream = GetResponseStream("bytes=0-499");
            Assert.AreEqual(500, stream.Length);
            Assert.AreEqual(206, Response.StatusCode);
            Assert.AreEqual($"bytes 0-499/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Test]
        public void Range_From_500b_to_899b()
        {
            var stream = GetResponseStream("bytes=500-899");
            Assert.AreEqual(400, stream.Length);
            Assert.AreEqual(206, Response.StatusCode);
            Assert.AreEqual($"bytes 500-899/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Test]
        public void Range_Last_300b()
        {
            var stream = GetResponseStream("bytes=-300");
            Assert.AreEqual(300, stream.Length);
            Assert.AreEqual(206, Response.StatusCode);
            var from = _file.Length - 300;
            var to = _file.Length - 1;

            Assert.AreEqual($"bytes {from}-{to}/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Test]
        public void Range_From_100b_toThe_End()
        {
            var stream = GetResponseStream($"bytes={(_file.Length - 100)}-");
            Assert.AreEqual(100, stream.Length);
            Assert.AreEqual(206, Response.StatusCode);
            var from = _file.Length - 100;
            var to = _file.Length - 1;
            Assert.AreEqual($"bytes {from}-{to}/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Test]
        public void Range_First_1b()
        {
            var stream = GetResponseStream("bytes=0-0");
            Assert.AreEqual(1, stream.Length);
            Assert.AreEqual(206, Response.StatusCode);
            Assert.AreEqual($"bytes 0-0/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Test]
        public void Range_Last_1b()
        {
            var stream = GetResponseStream("bytes=-1");
            Assert.AreEqual(1, stream.Length);
            Assert.AreEqual(206, Response.StatusCode);
            var from = _file.Length - 1;
            var to = _file.Length - 1;
            Assert.AreEqual($"bytes {from}-{to}/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Test]
        public void Range_Whole_File_With_RangeHeader()
        {
            var stream = GetResponseStream("bytes=0-" + (_file.Length - 1));
            Assert.AreEqual(_file.Length, stream.Length);
            Assert.AreEqual(206, Response.StatusCode);
            Assert.AreEqual($"bytes 0-{(_file.Length - 1)}/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Test]
        public void Range_Whole_File_Without_RangeHeader()
        {
            var stream = GetResponseStream(null);
            Assert.AreEqual(_file.Length, stream.Length);
            Assert.AreEqual(200, Response.StatusCode);
            Assert.AreEqual($"bytes 0-{(_file.Length - 1)}/{_file.Length}", Response.Headers[HttpHeaders.ContentRange]);
        }

        [Test]
        public void TransmissionRange_From_0_To_0()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-0";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            Assert.AreEqual(1, Response.OutputStream.Length);
            AssertBytes(_file, Response.OutputStream, 0, 1);
        }

        [Test]
        public void TransmissionRange_From_1_To_100()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=1-100";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            Assert.AreEqual(100, Response.OutputStream.Length);
            AssertBytes(_file, Response.OutputStream, 1, 100);
        }

        [Test]
        public void TransmissionRange_From_101_To_theEnd()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=101-";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            Assert.AreEqual(_file.Length - 101, Response.OutputStream.Length);
            AssertBytes(_file, Response.OutputStream, 101, (int)_file.Length);
        }

        [Test]
        public void TransmissionRange_WholeFile_WithRangeHeader()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            Assert.AreEqual(_file.Length, Response.OutputStream.Length);
            AssertBytes(_file, Response.OutputStream, 0, (int)_file.Length);
        }

        [Test]
        public void TransmissionRange_WholeFile_WithoutRangeHeader()
        {
            Request.Headers[HttpHeaders.Range] = null;
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);

            Assert.AreEqual(_file.Length, Response.OutputStream.Length);
            AssertBytes(_file, Response.OutputStream, 0, (int)_file.Length);
        }

        [Test]
        public void ShouldSend206If_Range_HeaderExists()
        {
            Request.Headers[HttpHeaders.Range] = "bytes=0-";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);
            Assert.AreEqual(206, Response.StatusCode);
        }

        [Test]
        public void ShouldSend200If_Range_HeaderDoesNotExist()
        {
            Request.Headers[HttpHeaders.Range] = null;
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);
            Assert.AreEqual(200, Response.StatusCode);
        }

        [Test]
        public void IfRangeHeader_Should_Be_Ignored_If_ItNotEquals_Etag()
        {
            Request.Headers[HttpHeaders.IfRange] = "ifRange fake header";
            var mock = new MockResumeFileResult(_file.FullName, Request);
            mock.WriteFileTest(Response);

            Assert.AreNotEqual(ResumeFileResult.Util.Etag(_file), Request.Headers[HttpHeaders.IfRange]);
            Assert.AreEqual(200, Response.StatusCode);
        }

        [Test]
        public void Etag_Should_Be_Added_To_Response_If_It_Equals_With_IfRange_In_Request()
        {
            var etag = ResumeFileResult.Util.Etag(_file);
            Request.Headers[HttpHeaders.IfRange] = etag;
            var mock = new MockResumeFileResult(_file.FullName, Request);
            mock.WriteFileTest(Response);
            Assert.AreEqual(Response.Headers[HttpHeaders.Etag], etag);
            Assert.AreEqual(200, Response.StatusCode);
        }

        [Test]
        public void Etag_Should_Be_Added_To_Response_If_It_Equals_With_IfRange_In_Request__PartialResponse()
        {
            var etag = ResumeFileResult.Util.Etag(_file);
            Request.Headers[HttpHeaders.IfRange] = etag;
            Request.Headers[HttpHeaders.Range] = "bytes=0-";
            new MockResumeFileResult(_file.FullName, Request).WriteFileTest(Response);
            Assert.AreEqual(Response.Headers[HttpHeaders.Etag], etag);
            Assert.IsNotNull(Response.Headers[HttpHeaders.ContentRange]);
            Assert.AreEqual(206, Response.StatusCode);
        }

        [Test]
        public void It_Should_Attach_Content_Disposition_If_There_Is_Download_File_Name()
        {
            new MockResumeFileResult(_file.FullName, Request, "test.name").WriteFileTest(Response);
            Assert.IsNotNull(Response.Headers[HttpHeaders.ContentDisposition]);
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
                for (var byteIndex = from ; byteIndex < to ; byteIndex++)
                {
                    var responseByte = responseStream.ReadByte();
                    var fileByte = fileStream.ReadByte();
                    Assert.AreEqual(responseByte, fileByte);
                }
            }
        }
    }
}