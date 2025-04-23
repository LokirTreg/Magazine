using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Shared.Protos;

namespace AsseblyClient.Services
{
    public class GrpcProductService
    {
        private readonly ProductService.ProductServiceClient _client;
        private readonly ILogger<GrpcProductService> _logger;

        public GrpcProductService(ProductService.ProductServiceClient client, ILogger<GrpcProductService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<ProductDetailResponse> GetProductDetailAsync(int productId)
        {
            try
            {
                _logger.LogInformation("Выполняется запрос GetProductDetailAsync для ProductId = {ProductId}", productId);

                var result = await _client.GetProductDetailAsync(
                    new GetProductDetailRequest { ProductId = productId });

                _logger.LogInformation("Успешно получен товар: {ProductTitle}", result.Product.Title);
                return result;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Ошибка получения товара с ID = {ProductId}", productId);
                throw new ApplicationException($"Ошибка получения товара: {ex.Status.Detail}", ex);
            }
        }

        public async Task<ProductDetailResponse> UpdateProductAsync(
            Product product,
            Seller seller,
            IEnumerable<WarehouseStock> warehouseStocks,
            IEnumerable<Country> countries)
        {
            try
            {
                _logger.LogInformation("Обновление товара {ProductName}", product.Title);

                var request = new UpdateProductRequest
                {
                    Product = product,
                    Seller = seller,
                    WarehouseStocks = { warehouseStocks },
                    AvailableCountries = { countries }
                };

                var response = await _client.UpdateProductAsync(request);

                _logger.LogInformation("Товар {ProductId} обновлён успешно", response.Product.Id);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Ошибка обновления товара {ProductId}", product.Id);
                throw new ApplicationException($"Ошибка обновления товара: {ex.Status.Detail}", ex);
            }
        }

        public async Task<ListProductsResponse> ListProductsAsync()
        {
            try
            {
                _logger.LogInformation("Получение списка товаров...");

                var response = await _client.ListProductsAsync(new ListProductsRequest());

                _logger.LogInformation("Список товаров получен. Количество: {Count}", response.Products.Count);
                return response;
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Ошибка получения списка товаров");
                throw new ApplicationException($"Ошибка получения списка товаров: {ex.Status.Detail}", ex);
            }
        }
    }
}
