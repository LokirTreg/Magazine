using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared.Protos;
using Magazine.Services;

namespace Magazine.ViewModels;
public partial class ProductsListViewModel : ObservableObject
{
    private readonly GrpcProductService _productService;

    [ObservableProperty]
    private List<Product> products = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    public ProductsListViewModel(GrpcProductService productService)
    {
        _productService = productService;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            var response = await _productService.ListProductsAsync();
            Products = response.Products.ToList();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
