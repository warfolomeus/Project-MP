//Модель товара. Отвечает за: Хранение всей информации о товаре и автоматические расчеты (цена, просрочка) 
using System;

namespace StockMasterCore.Models
{
    public class Product
    {
        public int Id { get; set; }                          // Уникальный идентификатор
        public string Name { get; set; }                     // Название товара (Рис, Макароны)
        public decimal BasePrice { get; set; }               // Базовая цена без скидки
        public string Description { get; set; }              // Дополнительная информация о товаре
        public decimal CurrentPrice => BasePrice * (1 - DiscountPercentage / 100); // Текущая цена со скидкой (ВЫЧИСЛЯЕМОЕ СВОЙСТВО)
        public decimal DiscountPercentage { get; set; }      // Процент скидки (0-100%)
        public int QuantityInStock { get; set; }             // Количество на складе
        public int MaxCapacity { get; set; }                 // Максимальная вместимость
        public int PackageSize { get; set; }                 // Количество единиц в оптовой упаковке (например, 20 пачек в 1 упаковке)
        public DateTime ExpiryDate { get; set; }             // Срок годности
        public int ReorderThreshold { get; set; }            // Порог для заказа новой поставки
        public bool IsExpired => ExpiryDate <= DateTime.Now; // Просрочен ли товар? (ВЫЧИСЛЯЕМОЕ СВОЙСТВО)
        public int DaysUntilExpiry => (ExpiryDate - DateTime.Now).Days; // Дней до истечения срока
    }
}