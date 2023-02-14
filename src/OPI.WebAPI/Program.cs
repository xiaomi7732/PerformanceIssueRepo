using System.Text.Json;
using Microsoft.Extensions.Logging.Console;
using OPI.WebAPI.RegistryStorage;
using OPI.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(opt => opt.AddDefaultPolicy(config =>{
    config.WithOrigins("http://localhost:5160", "https://white-bay-0b797e61e.2.azurestaticapps.net")
        .AllowAnyMethod()
        .WithHeaders("content-type");
}));

builder.Logging.AddSimpleConsole(opt =>
{
    opt.SingleLine = true;
    opt.ColorBehavior = LoggerColorBehavior.Enabled;
});

builder.Services.AddHttpClient("issue-spec", client =>
{
    client.BaseAddress = new Uri("https://raw.githubusercontent.com/xiaomi7732/PerformanceIssueRepo/");
});

builder.Services.AddSingleton<JsonSerializerOptions>(_ =>
{
    JsonSerializerOptions serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    return serializerOptions;
});

// Add services to the container.
builder.Services.AddTransient<IssueRegistryService>();
builder.Services.AddTransient<IssueItemService>();

#if DEBUG
builder.Services.AddTransient<LegacyIssueRegistryService>();
#endif

builder.Services.AddRegistryStorage();

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

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
