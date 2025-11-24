//Сбор статистики. Отвечает за: Сбор и хранение статистики за весь период работы
namespace StockMasterCore.Models
{
    public class WarehouseStatistics
    {
        public int CurrentDay { get; set; }                  // Текущий день симуляции
        public int TotalProductsSold { get; set; }           // Всего продано единиц товара
        public decimal TotalRevenue { get; set; }            // Общая выручка
        public decimal TotalDiscountLoss { get; set; }       // Потери из-за уценки
        public decimal TotalExpiredLoss { get; set; }        // Потери из-за просрочки
        public decimal TotalInventoryValue { get; set; }     // Общая стоимость остатков
        public void Reset()                                  // Обнуление данных
        {
            CurrentDay = 0;
            TotalProductsSold = 0;
            TotalRevenue = 0;
            TotalDiscountLoss = 0;
            TotalExpiredLoss = 0;
            TotalInventoryValue = 0;
        }
    }
}