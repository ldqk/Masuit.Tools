using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.IO;
using System.Text;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(@"F:\音乐\part1");
            using (var fs = new FileStream(@"D:\1.zip", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var archive = ZipArchive.Create())
                {
                    foreach (var file in files)
                    {
                        archive.AddEntry(Path.GetFileName(file), file);
                    }

                    archive.SaveTo(fs, new WriterOptions(CompressionType.Deflate)
                    {
                        LeaveStreamOpen = true,
                        ArchiveEncoding = new ArchiveEncoding()
                        {
                            Default = Encoding.UTF8
                        }
                    });
                }
            }
        }
    }
}