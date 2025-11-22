//Настройки симуляции. Отвечает за: Все параметры моделирования которые можно менять
namespace StockMasterCore.Models
{
    public class SimulationConfig
    {
        public int SimulationDays { get; set; } = 20;        // Длительность симуляции в днях
        public int StoreCount { get; set; } = 5;             // Количество магазинов
        public int ProductTypesCount { get; set; } = 15;     // Видов товаров
        public int DiscountDaysThreshold { get; set; } = 3;  // За сколько дней до конца срока делать скидку
        public double DiscountedProductOrderProbability { get; set; } = 0.7; // Вероятность заказа уцененного товара
        public int MinProductsPerOrder { get; set; } = 1; //Минимальное кол-во видов товаров в одном заказе
        public int MaxProductsPerOrder { get; set; } = 5; //Максимальное кол-во видов товаров в одном заказе
    }
}