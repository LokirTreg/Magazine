using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Shared.Protos;
using Server.Data;
using Microsoft.AspNetCore.Http;

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
            .AsNoTracking()
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

    public override async Task<ListProductsResponse> ListProducts(
        ListProductsRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Запрос списка товаров");

        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.WarehouseStocks)
                .ThenInclude(ws => ws.Warehouse)
            .Include(p => p.AvailableCountries)
            .ToListAsync();

        var response = new ListProductsResponse();
        response.Products.AddRange(products.Select(MapToResponseProduct));
        return response;
    }

    public override async Task<ProductDetailResponse> UpdateProduct(
        UpdateProductRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("Обновление товара ID: {ProductId}", request.Product.Id);

        var product = await _dbContext.Products
            .Include(p => p.Seller)
            .Include(p => p.WarehouseStocks)
                .ThenInclude(ws => ws.Warehouse)
            .Include(p => p.AvailableCountries)
            .FirstOrDefaultAsync(p => p.Id == request.Product.Id);

        if (product == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Товар с ID {request.Product.Id} не найден"));

        // Обновление основных полей
        product.Title = request.Product.Title;
        product.Description = request.Product.Description;
        product.Price = request.Product.Price;

        // Обновление продавца
        await UpdateSeller(product, request.Seller);

        // Обновление складских остатков
        await UpdateWarehouseStocks(product, request.WarehouseStocks);

        // Обновление стран
        await UpdateCountries(product, request.AvailableCountries);

        await _dbContext.SaveChangesAsync();
        return MapToResponse(product);
    }

    #region Helper Methods
    private Shared.Protos.Product MapToResponseProduct(Server.Data.Product product) => new()
    {
        Id = product.Id,
        Title = product.Title,
        Description = product.Description,
        Price = product.Price
    };

    private ProductDetailResponse MapToResponse(Server.Data.Product product) => new()
    {
        Product = MapToResponseProduct(product),
        Seller = new()
        {
            Id = product.Seller.Id,
            Name = product.Seller.Name,
            ContactEmail = product.Seller.ContactEmail
        },
        WarehouseStocks =
        {
            product.WarehouseStocks.Select(ws => new Shared.Protos.WarehouseStock
            {
                Warehouse = new() { Id = ws.Warehouse.Id, Address = ws.Warehouse.Address },
                Quantity = ws.Quantity
            })
        },
        AvailableCountries =
        {
            product.AvailableCountries.Select(c => new Shared.Protos.Country
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code
            })
        }
    };

    private async Task UpdateSeller(Server.Data.Product product, Shared.Protos.Seller protoSeller)
    {
        Id = protoSeller.Id,
        Name = protoSeller.Name,
        ContactEmail = protoSeller.ContactEmail
    };

    private async Task UpdateWarehouseStocks(
        Server.Data.Product product,
        IEnumerable<Shared.Protos.WarehouseStock> stocks)
    {
        var existingStocks = product.WarehouseStocks.ToList();
        var requestStockMap = stocks.ToDictionary(s => s.Warehouse.Id);

        // Удаление отсутствующих складов
        foreach (var existing in existingStocks)
        {
            if (!requestStockMap.ContainsKey(existing.WarehouseId))
            {
                product.WarehouseStocks.Remove(existing);
                _dbContext.Remove(existing);
            }
        }

        // Добавление/обновление складов
        foreach (var (warehouseId, protoStock) in requestStockMap)
        {
            var warehouse = await _dbContext.Warehouses.FindAsync(warehouseId)
                ?? throw new RpcException(new Status(StatusCode.NotFound, $"Склад {warehouseId} не найден"));

            var existingStock = product.WarehouseStocks
                .FirstOrDefault(ws => ws.WarehouseId == warehouseId);

            if (existingStock != null)
            {
                existingStock.Quantity = protoStock.Quantity;
            }
            else
            {
                product.WarehouseStocks.Add(new Server.Data.WarehouseStock
                {
                    ProductId = product.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = protoStock.Quantity
                });
            }
        }
    }

    private async Task UpdateCountries(
        Server.Data.Product product,
        IEnumerable<Shared.Protos.Country> protoCountries)
    {
        var countryIds = protoCountries.Select(c => c.Id).ToList();
        var countries = await _dbContext.Countries
            .Where(c => countryIds.Contains(c.Id))
            .ToListAsync();

        if (countries.Count != countryIds.Count)
            throw new RpcException(new Status(StatusCode.NotFound, "Одна или несколько стран не найдены"));

        product.AvailableCountries = countries;
    }
    #endregion
}
