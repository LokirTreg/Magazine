using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("MagazineDb")
          .EnableDetailedErrors()
          .EnableSensitiveDataLogging());

builder.Services.AddGrpc(options => {
    options.EnableDetailedErrors = true;
    options.MaxReceiveMessageSize = 10 * 1024 * 1024; // 10MB
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await SeedData.InitializeAsync(context);
}

app.MapGrpcService<ProductGrpcService>();
app.MapGet("/", () => "gRPC Server is running");
await app.RunAsync();

