using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Masuit.Tools.Reflection;
using Masuit.Tools.Systems;

namespace Masuit.Tools.Files.FileDetector;

public static class FileSignatureDetector
{
    private static List<IDetector> Detectors { get; set; } = [];

    public static IReadOnlyList<IDetector> Registered => Detectors;

    public static void AddDetector<T>() where T : IDetector
    {
        var instance = Activator.CreateInstance<T>();
        AddDetector(instance);
    }

    public static void AddDetector(IDetector instance)
    {
        if (!Detectors.Contains(instance))
        {
            Detectors.Add(instance);
        }
    }

    static FileSignatureDetector()
    {
        var type = typeof(IDetector);
        foreach (var item in AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.FullName.StartsWith(["System", "Microsoft"])).SelectMany(a => a.GetLoadableTypes()))
        {
            if (type.IsAssignableFrom(item) && !item.IsAbstract && !item.IsInterface && item.GetTypeInfo().DeclaredConstructors.First().GetParameters().Length == 0)
            {
                AddDetector(Activator.CreateInstance(item) as IDetector);
            }
        }
    }

    public static IDetector DetectFiletype(string filepath)
    {
        using var stream = File.OpenRead(filepath);
        return DetectFiletype(stream);
    }

    public static IDetector DetectFiletype(this FileInfo file)
    {
        using var stream = file.OpenRead();
        return DetectFiletype(stream);
    }

    public static IDetector DetectFiletype(this Stream stream)
    {
        if (stream.CanSeek && stream.Length > 0)
        {
            foreach (var detector in Detectors)
            {
                stream.Position = 0;
                if (detector.Detect(stream))
                {
                    stream.Position = 0;
                    return detector;
                }
            }
        }
        return new NoneDetector();
    }
}