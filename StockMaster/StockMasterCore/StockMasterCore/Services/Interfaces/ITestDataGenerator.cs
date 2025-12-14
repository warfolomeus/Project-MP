// Интерфейс генератора тестовых данных. Отвечает за: определение контракта для генерации тестовых данных
using StockMasterCore.Models;
using System;
using System.Collections.Generic;

namespace StockMasterCore.Services.Interfaces
{
    public interface ITestDataGenerator
    {
        // Основные методы
        List<Product> GenerateProducts(SimulationConfig config);
        List<Store> GenerateStores(SimulationConfig config);

        List<Product> GenerateProducts(SimulationConfig config, DateTime simulationStartDate);
        Order GenerateRandomOrder(Store store, List<Product> products, IWarehouseService service, int dayOffset);
        Order GenerateRandomOrder(Store store, List<Product> products, IWarehouseService service, DateTime orderDate);
        List<Order> GenerateDailyOrders(List<Store> stores, List<Product> products, IWarehouseService service, int dayOffset);
        List<Order> GenerateDailyOrders(List<Store> stores, List<Product> products, IWarehouseService service, DateTime orderDate);
    }
}