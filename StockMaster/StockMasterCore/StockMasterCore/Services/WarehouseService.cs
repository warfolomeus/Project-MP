// Координирует все процессы: прием заказов, отгрузку, списание, пополнение запасов, короче говоря - "мозг системы"
// Отвечает за: всю бизнес-логику работы склада, обработку заказов, управление запасами, уценку товаров
using StockMasterCore.Models;
using StockMasterCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockMasterCore.Services
{
    public class WarehouseService : IWarehouseService
    {
        private List<Product> _products;
        private List<Store> _stores;
        private List<Order> _orders;
        private List<SupplyRequest> _supplyRequests;
        private List<Order> _pendingShipments;

        private readonly IInventoryManager _inventoryManager;
        private readonly IOrderProcessor _orderProcessor;

        public SimulationConfig Config { get; set; }
        public WarehouseStatistics Statistics { get; private set; }
        public WarehouseSummary Summary { get; private set; }

        public WarehouseService(IInventoryManager inventoryManager, IOrderProcessor orderProcessor)
        {
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _orderProcessor = orderProcessor ?? throw new ArgumentNullException(nameof(orderProcessor));

            _products = new List<Product>();
            _stores = new List<Store>();
            _orders = new List<Order>();
            _supplyRequests = new List<SupplyRequest>();
            _pendingShipments = new List<Order>();

            Statistics = new WarehouseStatistics();
            Config = new SimulationConfig();
            Summary = new WarehouseSummary();
        }

        public WarehouseService() : this(new InventoryManager(), new OrderProcessor())
        {
        }

        public void InitializeWarehouse(List<Product> products, List<Store> stores)
        {
            _products = products ?? throw new ArgumentNullException(nameof(products));
            _stores = stores ?? throw new ArgumentNullException(nameof(stores));

            Statistics.Reset();
            UpdateSummary();
        }

        public void ProcessDay()
        {
            // 1. Обработка поставок от поставщиков
            _inventoryManager.ProcessDeliveries(_supplyRequests, _products, Config);

            // 2. Проверка и списание просроченных товаров
            _inventoryManager.CheckExpiredProducts(_products, Statistics);

            // 3. Автоматическое применение скидок
            ApplyAutomaticDiscounts();

            // 4. Обработка заказов за день
            ProcessDailyOrders();

            // 5. Проверка необходимости пополнения запасов
            _inventoryManager.CheckInventoryLevels(_products, _supplyRequests, Config);

            // 6. Обновление статистики и сводки
            UpdateStatistics();
            UpdateSummary();
        }

        public void ProcessSimulation(int days)
        {
            for (int day = 0; day < days; day++)
            {
                ProcessDay();
            }
        }

        public void ProcessOrder(Order order)
        {
            if (order == null) return;

            var processedOrder = _orderProcessor.ProcessOrder(order, _products, Statistics);
            if (processedOrder.IsProcessed)
            {
                _pendingShipments.Add(processedOrder);
            }
        }

        private void ProcessDailyOrders()
        {
            var todayOrders = _orders
                .Where(o => o.OrderDate.Date == DateTime.Now.Date && !o.IsProcessed)
                .ToList();

            var processedOrders = _orderProcessor.ProcessDailyOrders(todayOrders, _products, Statistics);

            foreach (var order in processedOrders.Where(o => o.IsProcessed))
            {
                _pendingShipments.Add(order);
            }
        }

        public void ApplyAutomaticDiscounts()
        {
            foreach (var product in _products)
            {
                if (product.DaysUntilExpiry <= Config.DiscountDaysThreshold &&
                    product.DaysUntilExpiry > 0 &&
                    product.QuantityInStock > 0 &&
                    product.DiscountPercentage == 0)
                {
                    // Автоматическая скидка: чем меньше дней, тем больше скидка
                    decimal discount = 0;
                    switch (product.DaysUntilExpiry)
                    {
                        case 1:
                            discount = 50; // 50% скидка если остался 1 день
                            break;
                        case 2:
                            discount = 30; // 30% скидка если осталось 2 дня
                            break;
                        case 3:
                            discount = 20; // 20% скидка если осталось 3 дня
                            break;
                        default:
                            discount = 0;
                            break;
                    }

                    if (discount > 0)
                    {
                        product.DiscountPercentage = discount;
                    }
                }
            }
        }

        // Выполнение заявки поставщика - пополнение запасов
        public void FulfillSupplyRequest(int requestId)
        {
            var request = _supplyRequests.FirstOrDefault(sr => sr.Id == requestId);
            if (request != null && !request.IsFulfilled)
            {
                var product = _products.FirstOrDefault(p => p.Id == request.ProductId);
                if (product != null)
                {
                    product.QuantityInStock += request.Quantity;
                    request.IsFulfilled = true;

                    // Обновляем срок годности для нового товара
                    product.ExpiryDate = DateTime.Now.AddDays(
                        new Random().Next(Config.MinExpiryDays, Config.MaxExpiryDays + 1));
                    product.DiscountPercentage = 0;
                }
            }
        }

        // Применение скидки к товару
        public void ApplyDiscount(int productId, decimal discountPercentage)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                product.DiscountPercentage = Math.Min(Math.Max(discountPercentage, 0), 100);
            }
        }

        // Получение списка товаров для уценки (скоро истекает срок)
        public List<DiscountProduct> GetProductsForDiscount()
        {
            return _products
                .Where(p => p.DaysUntilExpiry <= Config.DiscountDaysThreshold &&
                           p.DaysUntilExpiry > 0 &&
                           p.QuantityInStock > 0)
                .Select(p => new DiscountProduct
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    OriginalPrice = p.BasePrice,
                    DiscountedPrice = p.CurrentPrice,
                    DiscountPercentage = p.DiscountPercentage,
                    DaysUntilExpiry = p.DaysUntilExpiry,
                    CurrentStock = p.QuantityInStock
                })
                .ToList();
        }

        public List<Product> GetExpiringProducts()
        {
            return _products
                .Where(p => p.DaysUntilExpiry <= Config.DiscountDaysThreshold &&
                           p.QuantityInStock > 0)
                .ToList();
        }

        public List<Product> GetLowStockProducts()
        {
            return _products
                .Where(p => p.NeedsRestocking && p.QuantityInStock > 0)
                .ToList();
        }

        public void AddOrder(Order order)
        {
            if (order != null)
            {
                order.Id = _orders.Count + 1;
                _orders.Add(order);
            }
        }

        public void AddOrders(List<Order> orders)
        {
            if (orders != null)
            {
                foreach (var order in orders)
                {
                    order.Id = _orders.Count + 1;
                    _orders.Add(order);
                }
            }
        }

        public List<Product> GetProducts() => _products.ToList();
        public List<Store> GetStores() => _stores.ToList();
        public List<Order> GetPendingShipments() => _pendingShipments.ToList();
        public List<SupplyRequest> GetPendingSupplyRequests() =>
            _supplyRequests.Where(sr => !sr.IsFulfilled).ToList();
        public List<Order> GetTodayOrders() =>
            _orders.Where(o => o.OrderDate.Date == DateTime.Now.Date).ToList();
        public List<Order> GetAllOrders() => _orders.ToList();

        // Обновление ежедневной статистики
        private void UpdateStatistics()
        {
            Statistics.CurrentDay++;
            Statistics.TotalInventoryValue = _products.Sum(p => p.StockValue);
        }

        private void UpdateSummary()
        {
            Summary.TotalProducts = _products.Count;
            Summary.TotalStores = _stores.Count;
            Summary.TotalOrders = _orders.Count;
            Summary.ActiveProducts = _products.Count(p => p.QuantityInStock > 0);
            Summary.LowStockProducts = _products.Count(p => p.NeedsRestocking);
            Summary.ExpiringSoonProducts = _products.Count(p =>
                p.DaysUntilExpiry <= Config.DiscountDaysThreshold && p.DaysUntilExpiry > 0);
            Summary.PendingShipments = _pendingShipments.Count;
            Summary.PendingSupplyRequests = _supplyRequests.Count(sr => !sr.IsFulfilled);
        }
    }
}