using System;
using System.IO;
using Masuit.Tools.Files.FileDetector;

var detector = File.OpenRead(@"E:\下载\Part1.mkv").DetectFiletype();
Console.WriteLine(detector.Extension);