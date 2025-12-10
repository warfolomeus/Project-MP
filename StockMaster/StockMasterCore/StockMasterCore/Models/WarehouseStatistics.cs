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
        public decimal TotalLosses => TotalDiscountLoss + TotalExpiredLoss; // Общие убытки склада (от скидок + от списания)
        public decimal NetProfit => TotalRevenue - TotalLosses; // Чистая прибыль (выручка минус все убытки)
        public double AverageDailySales => CurrentDay > 0 ? (double)TotalProductsSold / CurrentDay : 0; // Средние продажи в день (общее количество ÷ дни работы)

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