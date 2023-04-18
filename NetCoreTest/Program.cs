using Masuit.Tools.AspNetCore.ModelBinder;

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
