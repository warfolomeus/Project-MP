// Настройки симуляции. Отвечает за: Все параметры моделирования которые можно менять
namespace StockMasterCore.Models
{
    public class SimulationConfig
    {
        // Основные параметры
        public int SimulationDays { get; set; } = 20;        // Длительность симуляции (10-30 дней)
        public int StoreCount { get; set; } = 5;             // Количество магазинов (3-9)
        public int ProductTypesCount { get; set; } = 15;     // Видов товаров (12-20)

        // Параметры уценки
        public int DiscountDaysThreshold { get; set; } = 3;  // За сколько дней до конца срока делать скидку
        public double DiscountedProductOrderProbability { get; set; } = 0.7; // Вероятность заказа уцененного товара (70%)

        // Параметры заказов
        public int MinProductsPerOrder { get; set; } = 1;    // Минимальное количество видов товаров в заказе
        public int MaxProductsPerOrder { get; set; } = 5;    // Максимальное количество видов товаров в заказе
        public int MinPackagesPerProduct { get; set; } = 1;  // Минимальное количество упаковок одного товара в заказе
        public int MaxPackagesPerProduct { get; set; } = 10; // Максимальное количество упаковок одного товара в заказе
        public double DailyOrderProbability { get; set; } = 0.8; // Вероятность что магазин сделает заказ в день (80%)

        // Параметры товаров
        public int MinProductPrice { get; set; } = 50;       // Минимальная цена товара (руб.)
        public int MaxProductPrice { get; set; } = 500;      // Максимальная цена товара (руб.)
        public int MinPackageSize { get; set; } = 5;         // Минимальный размер упаковки (ед. товара)
        public int MaxPackageSize { get; set; } = 25;        // Максимальный размер упаковки (ед. товара)
        public int MinExpiryDays { get; set; } = 1;          // Минимальный срок годности (дней)
        public int MaxExpiryDays { get; set; } = 30;         // Максимальный срок годности (дней)

        // Параметры склада
        public int MinProductCapacity { get; set; } = 50;    // Минимальная вместимость для товара
        public int MaxProductCapacity { get; set; } = 200;   // Максимальная вместимость для товара
        public int ReorderThresholdPercentage { get; set; } = 25; // Порог перезаказа (% от вместимости)
    }
}