// Координирует все процессы: прием заказов, отгрузку, списание, пополнение запасов, короче говоря - "мозг системы"
// Отвечает за: всю бизнес-логику работы склада, обработку заказов, управление запасами, уценку товаров
using StockMasterCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockMasterCore.Services
{
    public class WarehouseService
    {
        // Хранилища данных
        private List<Product> _products;           // Все товары на складе
        private List<Store> _stores;               // Все магазины-клиенты
        private List<Order> _orders;               // Все заказы от магазинов
        private List<SupplyRequest> _supplyRequests; // Заявки поставщикам на пополнение
        private List<Order> _pendingShipments;     // Готовые к отгрузке заказы

        private readonly Random _random = new Random();

        // Конфигурация системы и сбор статистики
        public SimulationConfig Config { get; set; }     // Настройки моделирования
        public WarehouseStatistics Statistics { get; private set; } // Статистика работы

        public WarehouseService()
        {
            // Инициализация пустых коллекций при создании сервиса
            _products = new List<Product>();
            _stores = new List<Store>();
            _orders = new List<Order>();
            _supplyRequests = new List<SupplyRequest>();
            _pendingShipments = new List<Order>();
            Statistics = new WarehouseStatistics();
            Config = new SimulationConfig();
        }

        // Инициализация склада начальными данными
        public void InitializeWarehouse(List<Product> products, List<Store> stores)
        {
            _products = products;
            _stores = stores;
            Statistics.Reset(); // Сброс данных при новой инициализации
        }

        // ОСНОВНОЙ МЕТОД - ОБРАБОТКА ОДНОГО РАБОЧЕГО ДНЯ
        public void ProcessDay()
        {
            // 1. Проверка просроченных товаров (самое первое - убрать испорченное)
            CheckExpiredProducts();

            // 2. Обработка заказов за день (основная деятельность)
            ProcessDailyOrders();

            // 3. Проверка необходимости пополнения запасов (планирование)
            CheckInventoryLevels();

            // 4. Обновление статистики (аналитика)
            UpdateStatistics();
        }

        // Проверка и списание просроченных товаров
        private void CheckExpiredProducts()
        {
            foreach (var product in _products.Where(p => p.IsExpired))
            {
                // Расчет убытков от списания и обновление статистики
                Statistics.TotalExpiredLoss += product.QuantityInStock * product.BasePrice;
                product.QuantityInStock = 0; // Полное списание просрочки
            }
        }

        // Обработка одного заказа от магазина
        public void ProcessOrder(Order order)
        {
            foreach (var item in order.Items)
            {
                var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null) continue;

                // РАСЧЕТ КОЛИЧЕСТВА УПАКОВОК ДЛЯ ОТГРУЗКИ:
                // Определяем сколько упаковок нужно для заказанного количества
                int packagesNeeded = (int)Math.Ceiling((double)item.RequestedQuantity / product.PackageSize);
                int availablePackages = product.QuantityInStock;

                // Берем минимум из "нужно" и "есть в наличии"
                int packagesToShip = Math.Min(packagesNeeded, availablePackages);
                item.PackagesToShip = packagesToShip;
                item.ActualQuantity = packagesToShip * product.PackageSize;

                // ОБНОВЛЕНИЕ ЗАПАСОВ НА СКЛАДЕ:
                product.QuantityInStock -= packagesToShip;

                // ОБНОВЛЕНИЕ СТАТИСТИКИ ПРОДАЖ:
                Statistics.TotalProductsSold += item.ActualQuantity;
                Statistics.TotalRevenue += item.ActualQuantity * product.CurrentPrice;

                // УЧЕТ ПОТЕРЬ ОТ УЦЕНКИ (если товар был со скидкой)
                if (product.DiscountPercentage > 0)
                {
                    Statistics.TotalDiscountLoss += item.ActualQuantity * (product.BasePrice - product.CurrentPrice);
                }
            }

            order.IsProcessed = true;
            _pendingShipments.Add(order); // Добавляем в список готовых к отгрузке
        }

        // Обработка всех заказов за текущий день
        private void ProcessDailyOrders()
        {
            var todayOrders = _orders.Where(o => o.OrderDate.Date == DateTime.Now.Date && !o.IsProcessed).ToList();

            foreach (var order in todayOrders)
            {
                ProcessOrder(order);
            }
        }

        // Проверка уровня запасов и создание заявок поставщикам
        private void CheckInventoryLevels()
        {
            foreach (var product in _products)
            {
                // Если товара мало и еще нет активной заявки на этот товар
                if (product.QuantityInStock <= product.ReorderThreshold &&
                    !_supplyRequests.Any(sr => sr.ProductId == product.Id && !sr.IsFulfilled))
                {
                    var supplyRequest = new SupplyRequest
                    {
                        Id = _supplyRequests.Count + 1,
                        ProductId = product.Id,
                        Quantity = product.MaxCapacity - product.QuantityInStock, // Дозаказываем до полной вместимости
                        RequestDate = DateTime.Now,
                        ExpectedDeliveryDate = DateTime.Now.AddDays(_random.Next(1, 6)), // Случайно 1-5 дней
                        IsFulfilled = false
                    };
                    _supplyRequests.Add(supplyRequest);
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
                    product.QuantityInStock += request.Quantity; // Увеличиваем остатки
                    request.IsFulfilled = true; // Помечаем как выполненную
                }
            }
        }

        // Применение скидки к товару
        public void ApplyDiscount(int productId, decimal discountPercentage)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                product.DiscountPercentage = Math.Min(discountPercentage, 100); // Не более 100%
            }
        }

        // Получение списка товаров для уценки (скоро истекает срок)
        public List<DiscountProduct> GetProductsForDiscount()
        {
            return _products
                .Where(p => p.DaysUntilExpiry <= Config.DiscountDaysThreshold && // Скоро истекает
                           p.DaysUntilExpiry > 0 &&                             // Но еще не просрочен
                           p.QuantityInStock > 0)                              // И есть в наличии
                .Select(p => new DiscountProduct
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    OriginalPrice = p.BasePrice,
                    DiscountedPrice = p.CurrentPrice,
                    DiscountPercentage = p.DiscountPercentage,
                    DaysUntilExpiry = p.DaysUntilExpiry
                })
                .ToList();
        }

        // МЕТОДЫ ДЛЯ ПОЛУЧЕНИЯ ДАННЫХ (интерфейс для будущей WPFки)

        public List<Product> GetProducts() => _products;
        public List<Store> GetStores() => _stores;
        public List<Order> GetPendingShipments() => _pendingShipments;
        public List<SupplyRequest> GetPendingSupplyRequests() => _supplyRequests.Where(sr => !sr.IsFulfilled).ToList();
        public List<Order> GetTodayOrders() => _orders.Where(o => o.OrderDate.Date == DateTime.Now.Date).ToList();

        // Обновление ежедневной статистики
        private void UpdateStatistics()
        {
            Statistics.CurrentDay++;
            Statistics.TotalInventoryValue = _products.Sum(p => p.QuantityInStock * p.CurrentPrice);
        }

        // Добавление нового заказа в систему
        public void AddOrder(Order order)
        {
            order.Id = _orders.Count + 1;
            _orders.Add(order);
        }
    }
}