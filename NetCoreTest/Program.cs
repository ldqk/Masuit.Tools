using Masuit.Tools.Media;

var remover = new ImageBorderRemover(ToleranceMode.DeltaECMC);
// var borders = remover.DetectBorders(@"F:\新建文件夹\[Graphis Gals] NO.570 Tsumugi Akari 明里䌷 Lovely Doll [121P]\gra_tsumugi-a6_008.jpg", 10);
remover.RemoveBorders(@"F:\新建文件夹\0T2A3259.jpg", @"F:\新建文件夹\0T2A3259_r.jpg",1);
//Console.ReadKey();