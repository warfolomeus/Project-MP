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
        public static IntegrationService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new IntegrationService();
                return _instance;
            }
        }

        public IWarehouseService WarehouseService { get; private set; }
        public ITestDataGenerator DataGenerator { get; private set; }

        public SimulationConfig Config { get; set; }
        public List<Product> Products { get; set; }
        public List<Store> Stores { get; set; }
        public DateTime CurrentDate { get; set; }
        public int CurrentDay { get; set; }
        public bool IsSimulationComplete { get; set; }

        private IntegrationService()
        {
            Config = new SimulationConfig();
            Products = new List<Product>();
            Stores = new List<Store>();
            CurrentDate = DateTime.Now.Date;
            CurrentDay = 0;
            IsSimulationComplete = false;

            WarehouseService = WarehouseServiceFactory.CreateWarehouseService();
            DataGenerator = WarehouseServiceFactory.CreateTestDataGenerator();

            WarehouseService.Config = Config;
        }

        public void GenerateTestData()
        {
            Products = DataGenerator.GenerateProducts(Config);
            Stores = DataGenerator.GenerateStores(Config);
            InitializeWarehouse();
            CurrentDay = 0;
            IsSimulationComplete = false;
        }

        public void InitializeWarehouse()
        {
            if (Products.Count > 0 && Stores.Count > 0)
            {
                WarehouseService.InitializeWarehouse(Products, Stores);
            }
        }

        public bool ProcessDay()
        {
            if (Products.Count == 0) return false;

            // Проверяем, не завершена ли симуляция
            if (CurrentDay >= Config.SimulationDays)
            {
                IsSimulationComplete = true;
                return false;
            }

            // Увеличиваем день
            CurrentDay++;
            CurrentDate = CurrentDate.AddDays(1);

            // 1. Обработка поставок от поставщиков
            ProcessDeliveries();

            // 2. Генерируем заказы на текущий день
            GenerateDailyOrders();

            // 3. Обрабатываем день через WarehouseService
            WarehouseService.ProcessDay();

            // 4. Обновляем наши данные
            Products = WarehouseService.GetProducts();

            // Проверяем завершение симуляции
            if (CurrentDay >= Config.SimulationDays)
            {
                IsSimulationComplete = true;
            }

            return true;
        }

        private void ProcessDeliveries()
        {
            var pendingRequests = WarehouseService.GetPendingSupplyRequests();
            foreach (var request in pendingRequests)
            {
                if (!request.IsFulfilled && request.ExpectedDeliveryDate.HasValue)
                {
                    if (request.ExpectedDeliveryDate.Value.Date <= CurrentDate.Date)
                    {
                        WarehouseService.FulfillSupplyRequest(request.Id);
                    }
                }
            }
        }

        private void GenerateDailyOrders()
        {
            // Генерируем заказы на текущий день
            var dailyOrders = DataGenerator.GenerateDailyOrders(
                Stores, Products, WarehouseService, CurrentDay);

            foreach (var order in dailyOrders)
            {
                order.OrderDate = CurrentDate;
                order.Id = GetNextOrderId();
            }

            if (dailyOrders.Any())
            {
                WarehouseService.AddOrders(dailyOrders);
            }
        }

        private int GetNextOrderId()
        {
            var allOrders = WarehouseService.GetAllOrders();
            return allOrders.Count > 0 ? allOrders.Max(o => o.Id) + 1 : 1;
        }

        public List<Order> GetTodayOrders()
        {
            return WarehouseService.GetAllOrders()
                .Where(o => o.OrderDate.Date == CurrentDate.Date)
                .ToList();
        }

        public List<SupplyRequest> GetPendingSupplyRequests()
        {
            return WarehouseService.GetPendingSupplyRequests()
                .Where(r => !r.IsFulfilled)
                .ToList();
        }

        public void ApplyDiscount(int productId, decimal discountPercentage)
        {
            WarehouseService.ApplyDiscount(productId, discountPercentage);
            Products = WarehouseService.GetProducts();
        }

        public void RemoveDiscount(int productId)
        {
            WarehouseService.ApplyDiscount(productId, 0);
            Products = WarehouseService.GetProducts();
        }

        public void Reset()
        {
            Config = new SimulationConfig();
            Products = new List<Product>();
            Stores = new List<Store>();
            CurrentDate = DateTime.Now.Date;
            CurrentDay = 0;
            IsSimulationComplete = false;

            WarehouseService = WarehouseServiceFactory.CreateWarehouseService();
            DataGenerator = WarehouseServiceFactory.CreateTestDataGenerator();

            WarehouseService.Config = Config;
        }
    }
}