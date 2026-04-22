using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WarhamerPaintInventoryWeb;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("Supabase", client =>
{
    client.BaseAddress = new Uri("https://xzxgnqguumibxgrcnfch.supabase.co");
    client.DefaultRequestHeaders.Add("apikey", "sb_publishable_XIotbEnq7xDcZ5BcsfYOVw_-RqGkY5E");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer sb_publishable_XIotbEnq7xDcZ5BcsfYOVw_-RqGkY5E");
});

builder.Services.AddScoped<InventoryManager>();

await builder.Build().RunAsync();
