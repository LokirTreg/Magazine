using CommunityToolkit.Mvvm.ComponentModel;
using Grpc.Core;
using Shared.Protos;

public partial class ProductViewModel : ObservableObject
{
    private readonly ProductService.ProductServiceClient _client;

    [ObservableProperty]
    private ProductDetailResponse _currentProduct = new();

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ProductViewModel(ProductService.ProductServiceClient client)
    {
        _client = client;
    }

    public async Task LoadProduct(int productId)
    {
        try
        {
            CurrentProduct = await _client.GetProductDetailAsync(
                new GetProductDetailRequest { ProductId = productId });
            StatusMessage = "Данные успешно загружены";
        }
        catch (RpcException ex)
        {
            StatusMessage = $"Ошибка: {ex.Status.Detail}";
        }
    }

    public async Task UpdateProduct()
    {
        try
        {
            var request = new UpdateProductRequest
            {
                Product = CurrentProduct.Product,
                Seller = CurrentProduct.Seller,
                WarehouseStocks = { CurrentProduct.WarehouseStocks },
                AvailableCountries = { CurrentProduct.AvailableCountries }
            };

            CurrentProduct = await _client.UpdateProductAsync(request);
            StatusMessage = "Данные успешно обновлены";
        }
        catch (RpcException ex)
        {
            StatusMessage = $"Ошибка обновления: {ex.Status.Detail}";
        }
    }
}
