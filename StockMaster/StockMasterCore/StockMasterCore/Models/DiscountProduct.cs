// Модель для работы с уценкой товаров. Отвечает за: Представление информации о товарах, которые подлежат уценке или уже уценены
namespace StockMasterCore.Models
{
    public class DiscountProduct
    {
        public int ProductId { get; set; }                   // ID товара для применения скидки
        public string ProductName { get; set; }              // Название товара для отображения
        public decimal OriginalPrice { get; set; }           // Цена без скидки (базовая)
        public decimal DiscountedPrice { get; set; }         // Цена со скидкой (текущая)
        public decimal DiscountPercentage { get; set; }      // Размер скидки в процентах
        public int DaysUntilExpiry { get; set; }             // Оставшееся количество дней до истечения срока
        public int CurrentStock { get; set; }                // Текущий остаток на складе
    }
}