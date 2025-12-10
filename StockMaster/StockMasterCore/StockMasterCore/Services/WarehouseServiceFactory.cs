// Фабрика сервисов. Отвечает за: централизованное создание объектов с зависимостями
using StockMasterCore.Services.Interfaces;

namespace StockMasterCore.Services
{
    public static class WarehouseServiceFactory
    {
        public static IWarehouseService CreateWarehouseService()
        {
            var inventoryManager = new InventoryManager();
            var orderProcessor = new OrderProcessor();
            return new WarehouseService(inventoryManager, orderProcessor);
        }

        public static ITestDataGenerator CreateTestDataGenerator()
        {
            return new TestDataGenerator();
        }

        public static IInventoryManager CreateInventoryManager()
        {
            return new InventoryManager();
        }

        public static IOrderProcessor CreateOrderProcessor()
        {
            return new OrderProcessor();
        }
    }
}