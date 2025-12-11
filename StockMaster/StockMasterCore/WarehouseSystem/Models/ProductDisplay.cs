namespace WarehouseApp.Models
{
    public class ProductDisplay
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DaysUntilExpiry { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal CurrentPrice { get; set; }
        public int QuantityInStock { get; set; }
    }
}