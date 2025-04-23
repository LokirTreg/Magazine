using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Http; // �������� ��� ���������
using Shared.Protos;
using Magazine.ViewModels;
using Client.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ����������� HttpClient
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
Console.WriteLine(builder.HostEnvironment.BaseAddress);
// ������������ gRPC
builder.Services
    .AddGrpcClient<ProductService.ProductServiceClient>(options =>
    {
        options.Address = new Uri("https://localhost:7264/");
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
        new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler())); // ���������� HttpClientHandler

// ����������� ViewModel
builder.Services.AddScoped<ProductViewModel>();
builder.Services.AddScoped<ProductsListViewModel>();

await builder.Build().RunAsync();
