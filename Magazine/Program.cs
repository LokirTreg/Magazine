using Client.Components;
using Grpc.Net.Client.Web;
using Magazine.ViewModels;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Shared.Protos;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Регистрация HttpClient
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("https://localhost:7095/") });


builder.Services
    .AddGrpcClient<ProductService.ProductServiceClient>(options =>
    {
        options.Address = new Uri("https://localhost:7264/");
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
        new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));

// Регистрация ViewModel
builder.Services.AddScoped<ProductViewModel>();
builder.Services.AddScoped<ProductsListViewModel>();

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger<Program>();
logger.LogInformation("Приложение запущено.");
await host.RunAsync();
