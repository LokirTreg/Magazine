using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Http; // Добавить эту директиву
using Shared.Protos;
using Magazine.ViewModels;
using Client.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Регистрация HttpClient
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
Console.WriteLine(builder.HostEnvironment.BaseAddress);
// Конфигурация gRPC
builder.Services
    .AddGrpcClient<ProductService.ProductServiceClient>(options =>
    {
        options.Address = new Uri("https://localhost:7264/");
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
        new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler())); // Используем HttpClientHandler

// Регистрация ViewModel
builder.Services.AddScoped<ProductViewModel>();
builder.Services.AddScoped<ProductsListViewModel>();

await builder.Build().RunAsync();
