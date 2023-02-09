using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OPI.Client;
using OPI.WebUI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// builder.Services.AddOPIClient(new Uri("https://opir-test.azurewebsites.net/"));
builder.Services.AddOPIClient(new Uri("http://localhost:5041/"));
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<CSVReaderFactory>();

await builder.Build().RunAsync();
