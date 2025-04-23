using System;

namespace Server.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        if (!context.Products.Any())
        {
            var seller = new Seller
            {
                Name = "TechCorp",
                ContactEmail = "sales@techcorp.com"
            };

            var warehouse = new Warehouse
            {
                Address = "Moscow, Lenina 42"
            };

            var country = new Country
            {
                Name = "Russia",
                Code = "RU"
            };

            var product = new Product
            {
                Title = "Smartphone X200",
                Description = "Флагманский смартфон",
                Price = 899.99,
                Seller = seller,
                SellerId = seller.Id,
                WarehouseStocks = new List<WarehouseStock>
                {
                    new() { Warehouse = warehouse, Quantity = 50 }
                },
                AvailableCountries = new List<Country> { country }
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();
        }
    }
}
