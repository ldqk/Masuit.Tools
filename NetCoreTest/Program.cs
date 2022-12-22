using Masuit.Tools.Systems;

var ms = new PooledMemoryStream();
ms.Write(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 0, 9);
foreach (var b in ms)
{
    Console.WriteLine(b);
}