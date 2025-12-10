// Модель сводной информации о складе
namespace StockMasterCore.Models
{
    public class WarehouseSummary
    {
        public int TotalProducts { get; set; }               // Всего видов товаров
        public int TotalStores { get; set; }                 // Всего магазинов
        public int TotalOrders { get; set; }                 // Всего заказов
        public int ActiveProducts { get; set; }              // Товаров в наличии
        public int LowStockProducts { get; set; }            // Товаров с низким запасом
        public int ExpiringSoonProducts { get; set; }        // Товаров скоро истекающих
        public int PendingShipments { get; set; }            // Готовых к отгрузке заказов
        public int PendingSupplyRequests { get; set; }       // Невыполненных заявок поставщикам
    }
}