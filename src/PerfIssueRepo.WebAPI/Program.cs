using System.Text.Json;
using Microsoft.Extensions.Logging.Console;
using PerfIssueRepo.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(opt =>
{
    opt.SingleLine = true;
    opt.ColorBehavior = LoggerColorBehavior.Enabled;
});

builder.Services.AddSingleton<JsonSerializerOptions>(_ =>
{
    JsonSerializerOptions serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    return serializerOptions;
});

// Add services to the container.
builder.Services.AddSingleton<IssueService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
