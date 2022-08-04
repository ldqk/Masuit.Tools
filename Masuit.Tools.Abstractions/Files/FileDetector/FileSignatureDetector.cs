using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Masuit.Tools.Files.FileDetector;

public static class FileSignatureDetector
{
    private static List<IDetector> Detectors { get; set; } = new();

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
        var detectorTypeInfo = typeof(IDetector).GetTypeInfo();
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.DefinedTypes))
        {
            if (detectorTypeInfo.IsAssignableFrom(type) && !type.IsAbstract && type.DeclaredConstructors.First().GetParameters().Length == 0)
            {
                AddDetector(Activator.CreateInstance(type.AsType()) as IDetector);
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
        string pre = null;
        IDetector foundDetector = new NoneDetector();
        while (true)
        {
            bool found = false;
            foreach (var detector in Detectors.Where(d => d.Precondition == pre))
            {
                stream.Position = 0;
                if (detector.Detect(stream))
                {
                    found = true;
                    foundDetector = detector;
                    pre = detector.Extension;
                    break;
                }
            }
            if (!found)
            {
                break;
            }
        }

        return foundDetector;
    }
}
