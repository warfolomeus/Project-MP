using StockMasterCore.Models;
using StockMasterCore.Services;
using StockMasterCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WarehouseApp.Services
{
    public class IntegrationService
    {
        private static IntegrationService _instance;
        public static IntegrationService Instance => _instance ?? (_instance = new IntegrationService());

        public IWarehouseService WarehouseService { get; private set; }
        public ITestDataGenerator TestDataGenerator { get; private set; }
        public SimulationConfig Config { get; set; }
        public List<Product> Products { get; private set; }
        public List<Store> Stores { get; private set; }
        public int CurrentDay { get; set; }
        public bool IsSimulationComplete { get; set; }
        public DateTime SimulationStartDate { get; private set; }

        public IntegrationService()
        {
            Config = new SimulationConfig();
            WarehouseService = WarehouseServiceFactory.CreateWarehouseService();
            TestDataGenerator = WarehouseServiceFactory.CreateTestDataGenerator();
            Products = new List<Product>();
            Stores = new List<Store>();
            CurrentDay = 0;
            IsSimulationComplete = false;
            SimulationStartDate = DateTime.Now;

            WarehouseService.Config = Config;
        }

        public void GenerateTestData()
        {
            Products = TestDataGenerator.GenerateProducts(Config);
            Stores = TestDataGenerator.GenerateStores(Config);
            WarehouseService.InitializeWarehouse(Products, Stores);
            CurrentDay = 0;
            IsSimulationComplete = false;
        }

        public void ApplyDiscount(int productId, decimal discountPercentage)
        {
            WarehouseService.ApplyDiscount(productId, discountPercentage);
        }

        public void RemoveDiscount(int productId)
        {
            WarehouseService.ApplyDiscount(productId, 0);
        }

        public bool ProcessDay()
        {
            if (IsSimulationComplete || CurrentDay >= Config.SimulationDays)
            {
                IsSimulationComplete = true;
                return false;
            }

            // Увеличиваем день перед обработкой
            CurrentDay++;

            // Очищаем ВСЕ старые заказы (обработанные) перед генерацией новых
            WarehouseService.ClearProcessedOrders();

            // Генерируем заказы на текущий день симуляции
            WarehouseService.GenerateDailyOrders(CurrentDay);

            // Обрабатываем день (списание, скидки, поставки)
            WarehouseService.ProcessDay();

            // Обновляем статистику
            WarehouseService.Statistics.CurrentDay = CurrentDay;

            if (CurrentDay >= Config.SimulationDays)
            {
                IsSimulationComplete = true;
            }

            return true;
        }

        public void ProcessOrderManually(int orderId)
        {
            WarehouseService.ProcessOrderManually(orderId);
        }

        public List<Order> GetTodayOrders()
        {
            return WarehouseService.GetTodayOrders();
        }

        public List<SupplyRequest> GetPendingSupplyRequests()
        {
            return WarehouseService.GetPendingSupplyRequests();
        }

        public List<Order> GetAllOrders()
        {
            return WarehouseService.GetAllOrders();
        }

        public void Reset()
        {
            Config = new SimulationConfig();
            WarehouseService = WarehouseServiceFactory.CreateWarehouseService();
            WarehouseService.Config = Config;
            Products.Clear();
            Stores.Clear();
            CurrentDay = 0;
            IsSimulationComplete = false;
            SimulationStartDate = DateTime.Now;
        }
    }
}