using System.ComponentModel.DataAnnotations;

namespace Server.Data;

public class Product
{
    [Key]
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }

    public Seller Seller { get; set; } = null!;
    public List<WarehouseStock> WarehouseStocks { get; set; } = new();
    public List<Country> AvailableCountries { get; set; } = new();
}

public class Seller
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string ContactEmail { get; set; } = null!;
}

public class WarehouseStock
{
    [Key]
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
}

public class Warehouse
{
    [Key]
    public int Id { get; set; }
    public string Address { get; set; } = null!;
}

public class Country
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
}
