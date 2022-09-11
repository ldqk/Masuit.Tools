using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.Files;
using Masuit.Tools.Media;

var stream = File.Open(@"D:\images\QQ½ØÍ¼20190923195408.jpg", FileMode.Open, FileAccess.ReadWrite);
var watermarker = new ImageWatermarker(stream);
var ms = watermarker.AddWatermark(File.OpenRead(@"D:\images\QQ½ØÍ¼20190923195408_¿´Í¼Íõ.png"), 0.5f);
ms.SaveFile(@"Y:\1.jpg");
Console.WriteLine(1);
Console.ReadKey();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options => options.ModelBinderProviders.InsertBodyOrDefaultBinding());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
