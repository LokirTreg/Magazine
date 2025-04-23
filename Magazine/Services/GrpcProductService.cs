using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Shared.Protos;

namespace Magazine.Services
{
    public class GrpcProductService
	{
		private readonly ProductService.ProductServiceClient _client;
		private readonly HttpClient _httpClient;
		private readonly string _baseAddress;

		public GrpcProductService(HttpClient httpClient, NavigationManager navigation)
		{
			_httpClient = httpClient;
			_baseAddress = navigation.BaseUri;

			var channel = GrpcChannel.ForAddress(_baseAddress, new GrpcChannelOptions
			{
				HttpHandler = new GrpcWebHandler(new HttpClientHandler())
			});

			_client = new ProductService.ProductServiceClient(channel);
		}

		public async Task<ProductDetailResponse> GetProductDetailAsync(int productId)
		{
			try
			{
				return await _client.GetProductDetailAsync(
					new GetProductDetailRequest { ProductId = productId });
			}
			catch (RpcException ex)
			{
				throw new ApplicationException(
					$"Ошибка получения товара: {ex.Status.Detail}", ex);
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
				var request = new UpdateProductRequest
				{
					Product = product,
					Seller = seller,
					WarehouseStocks = { warehouseStocks },
					AvailableCountries = { countries }
				};

				return await _client.UpdateProductAsync(request);
			}
			catch (RpcException ex)
			{
				throw new ApplicationException(
					$"Ошибка обновления товара: {ex.Status.Detail}", ex);
			}
		}

		public async Task<ListProductsResponse> ListProductsAsync()
		{
			try
			{
				return await _client.ListProductsAsync(new ListProductsRequest());
			}
			catch (RpcException ex)
			{
				throw new ApplicationException(
					$"Ошибка получения списка товаров: {ex.Status.Detail}", ex);
			}
		}
	}
}
