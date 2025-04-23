using AsseblyClient;
using AsseblyClient.Services;
using AsseblyClient.ViewModels;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Shared.Protos;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("https://localhost:7264") });

builder.Services.AddScoped(sp =>
{
    var grpcHandler = new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler());

    var channel = GrpcChannel.ForAddress("https://localhost:7264", new GrpcChannelOptions
    {
        HttpHandler = grpcHandler
    });

    return new ProductService.ProductServiceClient(channel);
});
builder.Services.AddScoped<GrpcProductService>();

builder.Services.AddScoped<ProductViewModel>();
builder.Services.AddScoped<ProductsListViewModel>();

await builder.Build().RunAsync();
