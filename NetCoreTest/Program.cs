using Masuit.Tools.Media;

var remover = new ImageBorderRemover(ToleranceMode.DeltaE1994);
remover.RemoveBorders(@"F:\新建文件夹\0T2A3259.jpg", @"F:\新建文件夹\0T2A3259_r.jpg",2);
//Console.ReadKey();