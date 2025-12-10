// Модель товара. Отвечает за: Хранение всей информации о товаре и автоматические расчеты (цена, просрочка)
using System;

namespace StockMasterCore.Models
{
    public class Product
    {
        public int Id { get; set; }                          // Уникальный идентификатор
        public string Name { get; set; }                     // Название товара (Рис, Макароны)
        public string Description { get; set; }              // Дополнительная информация о товаре
        public decimal BasePrice { get; set; }               // Базовая цена без скидки
        public decimal CurrentPrice => BasePrice * (1 - DiscountPercentage / 100); // Текущая цена со скидкой
        public decimal DiscountPercentage { get; set; }      // Процент скидки (0-100%)
        public int QuantityInStock { get; set; }             // Количество на складе
        public int MaxCapacity { get; set; }                 // Максимальная вместимость
        public int PackageSize { get; set; }                 // Количество единиц в оптовой упаковке
        public DateTime ExpiryDate { get; set; }             // Срок годности
        public int ReorderThreshold { get; set; }            // Порог для заказа новой поставки
        public bool IsExpired => ExpiryDate <= DateTime.Now; // Просрочен ли товар?
        public int DaysUntilExpiry => (ExpiryDate - DateTime.Now).Days; // Дней до истечения срока
        public bool NeedsRestocking => QuantityInStock <= ReorderThreshold; // Проверяет, нужно ли заказывать товар у поставщика
        public bool IsDiscounted => DiscountPercentage > 0; // Проверяет, есть ли на товар скидка
        public decimal StockValue => QuantityInStock * CurrentPrice; // Рассчитывает общую стоимость всех единиц этого товара на складе
        public double CapacityPercentage => MaxCapacity > 0 ? (double)QuantityInStock / MaxCapacity * 100 : 0; // Рассчитывает процент заполнения склада этим товаром
    }
}