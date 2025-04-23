using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Shared.Protos;
using Shared;
using Server.Data;
using Microsoft.AspNetCore.Http;
using System;

namespace Server.Services;

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly ILogger<ProductGrpcService> _logger;
    private readonly AppDbContext _dbContext;

    public ProductGrpcService(
        ILogger<ProductGrpcService> logger,
        AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public override async Task<ProductDetailResponse> GetProductDetail(
        GetProductDetailRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Запрос товара ID: {ProductId}", request.ProductId);

        var product = await _dbContext.Products
            .Include(p => p.Seller)
            .Include(p => p.WarehouseStocks)
                .ThenInclude(ws => ws.Warehouse)
            .Include(p => p.AvailableCountries)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);

        return product == null
            ? throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Товар с ID {request.ProductId} не найден"))
            : MapToResponse(product);
    }

    public override async Task<ProductDetailResponse> UpdateProduct(
        UpdateProductRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Обновление товара ID: {ProductId}", request.Product.Id);

        var product = await _dbContext.Products
            .Include(p => p.Seller)
            .Include(p => p.WarehouseStocks)
            .Include(p => p.AvailableCountries)
            .FirstOrDefaultAsync(p => p.Id == request.Product.Id);

        if (product == null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Товар с ID {request.Product.Id} не найден"));
        }

        // Обновление основных полей
        product.Title = request.Product.Title;
        product.Description = request.Product.Description;
        product.Price = request.Product.Price;

        // Обновление связанных данных
        product.Seller = MapSeller(request.Seller);
        UpdateWarehouseStocks(product, request.WarehouseStocks);
        UpdateCountries(product, request.AvailableCountries);

        await _dbContext.SaveChangesAsync();

        return MapToResponse(product);
    }

    private ProductDetailResponse MapToResponse(Server.Data.Product product) => new()
    {
        Product = new Shared.Protos.Product
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price
        },
        Seller = new Shared.Protos.Seller
        {
            Id = product.Seller.Id,
            Name = product.Seller.Name,
            ContactEmail = product.Seller.ContactEmail
        },
        WarehouseStocks = { product.WarehouseStocks.Select(ws =>
            new Shared.Protos.WarehouseStock
            {
                Warehouse = new Shared.Protos.Warehouse
                {
                    Id = ws.Warehouse.Id,
                    Address = ws.Warehouse.Address
                },
                Quantity = ws.Quantity
            })},
        AvailableCountries = { product.AvailableCountries.Select(c =>
            new Shared.Protos.Country
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code
            })}
    };

    private Server.Data.Seller MapSeller(Shared.Protos.Seller protoSeller) => new()
    {
        Id = protoSeller.Id,
        Name = protoSeller.Name,
        ContactEmail = protoSeller.ContactEmail
    };

    private void UpdateWarehouseStocks(Server.Data.Product product,
        IEnumerable<Shared.Protos.WarehouseStock> stocks)
    {
        product.WarehouseStocks.Clear();
        foreach (var stock in stocks)
        {
            product.WarehouseStocks.Add(new Server.Data.WarehouseStock
            {
                Warehouse = new Server.Data.Warehouse
                {
                    Id = stock.Warehouse.Id,
                    Address = stock.Warehouse.Address
                },
                Quantity = stock.Quantity
            });
        }
    }

    private void UpdateCountries(Server.Data.Product product,
        IEnumerable<Shared.Protos.Country> countries)
    {
        product.AvailableCountries.Clear();
        foreach (var country in countries)
        {
            product.AvailableCountries.Add(new Server.Data.Country
            {
                Id = country.Id,
                Name = country.Name,
                Code = country.Code
            });
        }
    }
}
