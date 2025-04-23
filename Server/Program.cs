using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("MagazineDb")
           .EnableDetailedErrors()
           .EnableSensitiveDataLogging());

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 10 * 1024 * 1024;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Grpc-Status", "Grpc-Message");
    });
});

var app = builder.Build();

app.UseRouting();

app.UseGrpcWeb();
app.UseCors("AllowAll");

app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<ProductGrpcService>().EnableGrpcWeb();

    endpoints.MapGet("/", () => "gRPC Server is running");
});

// инициализация базы
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.InitializeAsync(context);
}
app.MapGet("/products", async (AppDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
});
await app.RunAsync();
