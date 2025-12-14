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
        private ITestDataGenerator _testDataGenerator;

        private readonly IInventoryManager _inventoryManager;
        private readonly IOrderProcessor _orderProcessor;

        // Словарь для отслеживания оставшихся дней годности товаров
        private Dictionary<int, int> _productExpiryDays = new Dictionary<int, int>();

        public SimulationConfig Config { get; set; }
        public WarehouseStatistics Statistics { get; private set; }
        public WarehouseSummary Summary { get; private set; }

        public WarehouseService(IInventoryManager inventoryManager, IOrderProcessor orderProcessor)
        {
            _inventoryManager = inventoryManager ?? throw new ArgumentNullException(nameof(inventoryManager));
            _orderProcessor = orderProcessor ?? throw new ArgumentNullException(nameof(orderProcessor));
            _testDataGenerator = new TestDataGenerator();

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

            // Инициализируем словарь дней годности
            _productExpiryDays.Clear();
            foreach (var product in _products)
            {
                // Сохраняем начальные дни годности
                int initialDays = Math.Max(product.DaysUntilExpiry, 0);
                _productExpiryDays[product.Id] = initialDays;

                // Обновляем ExpiryDate для корректного отображения в первый день
                product.ExpiryDate = DateTime.Now.AddDays(initialDays);
            }

            Statistics.Reset();
            UpdateSummary();
        }

        // Получить оставшиеся дни годности для товара
        public int GetProductExpiryDays(int productId)
        {
            return _productExpiryDays.ContainsKey(productId) ? _productExpiryDays[productId] : 0;
        }

        // Уменьшить срок годности всех товаров на 1 день
        private void DecreaseExpiryDays()
        {
            foreach (var productId in _productExpiryDays.Keys.ToList())
            {
                if (_productExpiryDays[productId] > 0)
                {
                    _productExpiryDays[productId]--;

                    // Обновляем ExpiryDate в продукте для корректного отображения
                    var product = _products.FirstOrDefault(p => p.Id == productId);
                    if (product != null)
                    {
                        product.ExpiryDate = DateTime.Now.AddDays(_productExpiryDays[productId]);
                    }
                }
            }
        }

        // Обновить срок годности для нового товара (при поставке)
        private void UpdateExpiryDaysForNewProduct(int productId, int days)
        {
            _productExpiryDays[productId] = Math.Max(days, 0);

            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                product.ExpiryDate = DateTime.Now.AddDays(days);
            }
        }

        public void ClearProcessedOrders()
        {
            // Удаляем все обработанные заказы
            var processedOrders = _orders.Where(o => o.IsProcessed).ToList();
            foreach (var order in processedOrders)
            {
                _orders.Remove(order);
            }
            UpdateSummary();
        }

        public void GenerateDailyOrders(int dayOffset)
        {
            if (_stores == null || !_stores.Any() || _products == null || !_products.Any())
                return;

            // Очищаем старые заказы (обработанные и необработанные) перед генерацией новых
            ClearProcessedOrders();
            var oldUnprocessedOrders = _orders.Where(o => !o.IsProcessed).ToList();
            foreach (var order in oldUnprocessedOrders)
            {
                _orders.Remove(order);
            }

            // Используем дату симуляции
            var dailyOrders = _testDataGenerator.GenerateDailyOrders(_stores, _products, this, dayOffset);

            foreach (var order in dailyOrders)
            {
                if (order != null && order.Items.Any() && order.TotalAmount > 0)
                {
                    order.Id = _orders.Count + 1;
                    _orders.Add(order);
                }
            }

            UpdateSummary();
        }

        public void ProcessDay()
        {
            // 1. Уменьшаем срок годности всех товаров на 1 день
            DecreaseExpiryDays();

            // 2. Обработка поставок от поставщиков
            _inventoryManager.ProcessDeliveries(_supplyRequests, _products, Config);

            // 3. Проверка и списание просроченных товаров (используем наш словарь)
            CheckExpiredProducts();

            // 4. Автоматическое применение скидок (используем наш словарь)
            ApplyAutomaticDiscounts();

            // 5. Проверка необходимости пополнения запасов
            _inventoryManager.CheckInventoryLevels(_products, _supplyRequests, Config);

            // 6. Обновление статистики и сводки
            UpdateStatistics();
            UpdateSummary();
        }

        // Проверка просроченных товаров с использованием нашего словаря
        private void CheckExpiredProducts()
        {
            var expiredProductIds = _productExpiryDays
                .Where(kv => kv.Value <= 0 && _products.Any(p => p.Id == kv.Key && p.QuantityInStock > 0))
                .Select(kv => kv.Key)
                .ToList();

            foreach (var productId in expiredProductIds)
            {
                var product = _products.FirstOrDefault(p => p.Id == productId);
                if (product != null && product.QuantityInStock > 0)
                {
                    decimal loss = product.QuantityInStock * product.BasePrice;
                    Statistics.TotalExpiredLoss += loss;
                    product.QuantityInStock = 0;
                }
            }
        }

        // Автоматическое применение скидок с использованием нашего словаря
        public void ApplyAutomaticDiscounts()
        {
            foreach (var product in _products)
            {
                int daysUntilExpiry = GetProductExpiryDays(product.Id);

                if (daysUntilExpiry <= Config.DiscountDaysThreshold &&
                    daysUntilExpiry > 0 &&
                    product.QuantityInStock > 0 &&
                    product.DiscountPercentage == 0)
                {
                    // Автоматическая скидка: чем меньше дней, тем больше скидка
                    decimal discount = 0;
                    switch (daysUntilExpiry)
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

        public Order ProcessOrderManually(int orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId && !o.IsProcessed);
            if (order == null) return null;

            // Создаем копию заказа для обработки
            var orderCopy = new Order
            {
                Id = order.Id,
                StoreId = order.StoreId,
                OrderDate = order.OrderDate,
                IsProcessed = false,
                TotalAmount = 0,
                Items = new List<OrderItem>()
            };

            foreach (var item in order.Items)
            {
                orderCopy.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    RequestedQuantity = item.RequestedQuantity,
                    ActualQuantity = item.ActualQuantity,
                    PackagesToShip = item.PackagesToShip
                });
            }

            // Используем ProcessOrder для фактической обработки
            var processedOrder = _orderProcessor.ProcessOrder(orderCopy, _products, Statistics);

            if (processedOrder != null && processedOrder.IsProcessed)
            {
                // Заказ успешно обработан
                _pendingShipments.Add(processedOrder);

                // Помечаем оригинальный заказ как обработанный
                order.IsProcessed = true;

                UpdateSummary();
                UpdateStatistics();

                return processedOrder;
            }
            else
            {
                // Заказ не может быть выполнен
                // Не удаляем его, оставляем необработанным
                return null;
            }
        }

        public void ProcessOrder(Order order)
        {
            if (order == null) return;

            var processedOrder = _orderProcessor.ProcessOrder(order, _products, Statistics);
            if (processedOrder != null && processedOrder.IsProcessed)
            {
                _pendingShipments.Add(processedOrder);
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
                    var random = new Random();
                    int newExpiryDays = random.Next(Config.MinExpiryDays, Config.MaxExpiryDays + 1);
                    UpdateExpiryDaysForNewProduct(product.Id, newExpiryDays);
                    product.DiscountPercentage = 0;

                    // Пересчитываем ожидающие заказы с новыми остатками
                    RecalculatePendingOrders();
                }
            }
        }

        // Пересчет ожидающих заказов
        private void RecalculatePendingOrders()
        {
            var pendingOrders = _orders.Where(o => !o.IsProcessed).ToList();

            foreach (var order in pendingOrders)
            {
                decimal newTotal = 0;
                bool hasItems = false;

                foreach (var item in order.Items)
                {
                    var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null) continue;

                    // Рассчитываем сколько можем отгрузить сейчас
                    int packagesToShip = CalculatePackagesToShip(product, item.RequestedQuantity);
                    int actualQuantity = packagesToShip * product.PackageSize;

                    item.ActualQuantity = actualQuantity;
                    item.PackagesToShip = packagesToShip;

                    if (actualQuantity > 0)
                    {
                        newTotal += actualQuantity * product.CurrentPrice;
                        hasItems = true;
                    }
                }

                order.TotalAmount = newTotal;

                // Если в заказе нет товаров, удаляем его
                if (!hasItems)
                {
                    _orders.Remove(order);
                }
            }
        }

        private int CalculatePackagesToShip(Product product, int requestedQuantity)
        {
            if (product == null || product.QuantityInStock <= 0) return 0;

            int packagesNeeded = (int)Math.Ceiling((double)requestedQuantity / product.PackageSize);
            int availablePackages = product.QuantityInStock / product.PackageSize;
            return Math.Min(packagesNeeded, availablePackages);
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
                .Where(p =>
                {
                    int days = GetProductExpiryDays(p.Id);
                    return days <= Config.DiscountDaysThreshold &&
                           days > 0 &&
                           p.QuantityInStock > 0;
                })
                .Select(p => new DiscountProduct
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    OriginalPrice = p.BasePrice,
                    DiscountedPrice = p.CurrentPrice,
                    DiscountPercentage = p.DiscountPercentage,
                    DaysUntilExpiry = GetProductExpiryDays(p.Id),
                    CurrentStock = p.QuantityInStock
                })
                .ToList();
        }

        public List<Product> GetExpiringProducts()
        {
            return _products
                .Where(p =>
                {
                    int days = GetProductExpiryDays(p.Id);
                    return days <= Config.DiscountDaysThreshold &&
                           days > 0 &&
                           p.QuantityInStock > 0;
                })
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
            if (order != null && order.Items.Any() && order.TotalAmount > 0)
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
                    if (order != null && order.Items.Any() && order.TotalAmount > 0)
                    {
                        order.Id = _orders.Count + 1;
                        _orders.Add(order);
                    }
                }
            }
        }

        public List<Product> GetProducts() => _products.ToList();
        public List<Store> GetStores() => _stores.ToList();
        public List<Order> GetPendingShipments() => _pendingShipments.ToList();
        public List<SupplyRequest> GetPendingSupplyRequests() =>
            _supplyRequests.Where(sr => !sr.IsFulfilled).ToList();
        public List<Order> GetTodayOrders() => _orders.Where(o => !o.IsProcessed && o.TotalAmount > 0).ToList();
        public List<Order> GetAllOrders() => _orders.Where(o => !o.IsProcessed).ToList();

        // Обновление ежедневной статистики
        private void UpdateStatistics()
        {
            Statistics.TotalInventoryValue = _products.Sum(p => p.StockValue);
        }

        private void UpdateSummary()
        {
            Summary.TotalProducts = _products.Count;
            Summary.TotalStores = _stores.Count;
            Summary.TotalOrders = _orders.Count;
            Summary.ActiveProducts = _products.Count(p => p.QuantityInStock > 0);
            Summary.LowStockProducts = _products.Count(p => p.NeedsRestocking);

            // Обновляем количество товаров скоро истекающих (используем наш словарь)
            Summary.ExpiringSoonProducts = _products.Count(p =>
            {
                int days = GetProductExpiryDays(p.Id);
                return days <= Config.DiscountDaysThreshold && days > 0 && p.QuantityInStock > 0;
            });

            Summary.PendingShipments = _pendingShipments.Count(o => !o.IsProcessed);
            Summary.PendingSupplyRequests = _supplyRequests.Count(sr => !sr.IsFulfilled);
        }

        public void ProcessSimulation(int days)
        {
            for (int day = 0; day < days; day++)
            {
                GenerateDailyOrders(day);
                ProcessDay();
            }
        }
    }
}