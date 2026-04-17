using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend;
using Frontend.Models;
using Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient pointing ONLY to the API Gateway
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5050")   // CHANGE THIS TO YOUR ACTUAL GATEWAY PORT
});

// Register ApiService
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
