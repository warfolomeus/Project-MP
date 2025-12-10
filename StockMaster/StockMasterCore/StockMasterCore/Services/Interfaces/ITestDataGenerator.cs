// Интерфейс генератора тестовых данных. Отвечает за: определение контракта для генерации тестовых данных
using StockMasterCore.Models;
using System.Collections.Generic;

namespace StockMasterCore.Services.Interfaces
{
    public interface ITestDataGenerator
    {
        List<Product> GenerateProducts(SimulationConfig config);           // Генерация товаров
        List<Store> GenerateStores(SimulationConfig config);               // Генерация магазинов
        Order GenerateRandomOrder(Store store, List<Product> products, IWarehouseService service, int dayOffset); // Генерация случайного заказа
        List<Order> GenerateDailyOrders(List<Store> stores, List<Product> products, IWarehouseService service, int dayOffset); // Генерация заказов на день
    }
}