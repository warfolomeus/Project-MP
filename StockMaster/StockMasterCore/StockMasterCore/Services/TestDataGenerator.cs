// Генератор тестовых данных. Отвечает за: создание реалистичных данных для тестирования
using StockMasterCore.Models;
using StockMasterCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockMasterCore.Services
{
    public class TestDataGenerator : ITestDataGenerator
    {
        // Генератор случайных чисел для создания разнообразных данных
        private readonly Random _random = new Random();

        // Справочник названий продуктов для генерации товаров
        // Содержит 15 популярных продуктовых наименований как требуется по условию (12≤ K ≤20)
        private readonly string[] _productNames =
        {
            "Рис", "Макароны", "Мука", "Сахар", "Соль",
            "Масло подсолнечное", "Чай", "Кофе", "Печенье", "Шоколад",
            "Консервы мясные", "Консервы рыбные", "Соки", "Вода", "Молоко",
            "Хлеб", "Яйца", "Масло сливочное", "Сыр", "Йогурт"
        };

        public List<Product> GenerateProducts(SimulationConfig config)
        {
            var products = new List<Product>();

            for (int i = 0; i < config.ProductTypesCount; i++)
            {
                int maxCapacity = _random.Next(config.MinProductCapacity, config.MaxProductCapacity + 1);
                int packageSize = _random.Next(config.MinPackageSize, config.MaxPackageSize + 1);

                products.Add(new Product
                {
                    Id = i + 1,
                    Name = _productNames[i % _productNames.Length],
                    Description = $"Продукт {_productNames[i % _productNames.Length]}",
                    BasePrice = (decimal)(_random.Next(config.MinProductPrice, config.MaxProductPrice) + _random.NextDouble()),
                    QuantityInStock = _random.Next(0, maxCapacity + 1),
                    MaxCapacity = maxCapacity,
                    PackageSize = packageSize,
                    ExpiryDate = DateTime.Now.AddDays(_random.Next(config.MinExpiryDays, config.MaxExpiryDays + 1)),
                    ReorderThreshold = maxCapacity * config.ReorderThresholdPercentage / 100,
                    DiscountPercentage = 0
                });
            }

            return products;
        }

        public List<Store> GenerateStores(SimulationConfig config)
        {
            var stores = new List<Store>();
            var storeNames = new[] { "Магазин", "Супермаркет", "Палатка", "Торговая точка", "Мини-маркет" };
            var streets = new[] { "Ленина", "Нахимсона", "Советская", "Труфанова", "Победы", "Гагарина", "Кирова", "Союзная" };

            for (int i = 0; i < config.StoreCount; i++)
            {
                stores.Add(new Store
                {
                    Id = i + 1,
                    Name = $"{storeNames[i % storeNames.Length]} №{i + 1}",
                    Address = $"ул. {streets[i % streets.Length]}, д. {_random.Next(1, 100)}",
                    ContactPerson = $"Менеджер {i + 1}"
                });
            }

            return stores;
        }

        // Генерация случайного заказа от торговой точки
        public Order GenerateRandomOrder(Store store, List<Product> products, IWarehouseService service, int dayOffset)
        {
            var order = new Order
       
            {
                StoreId = store.Id,
                OrderDate = DateTime.Now.AddDays(dayOffset)
            };

            // ЛОГИКА ВЫБОРА ТОВАРОВ ДЛЯ ЗАКАЗА:
            // По условию: "вероятность заказа уцененных продуктов выше, чем неуцененных"

            // Получаем список уцененных товаров
            var discountedProducts = service.GetProductsForDiscount();

            // Решаем: заказывать уцененные товары или все подряд
            // Если есть уцененные товары и случайное число меньше вероятности заказа уцененных
            var productsToOrderFrom = discountedProducts.Any() &&
                                    _random.NextDouble() < service.Config.DiscountedProductOrderProbability
                ? products.Where(p => p.DiscountPercentage > 0 && p.QuantityInStock > 0).ToList()
                : products.Where(p => p.QuantityInStock > 0).ToList();

            // Если нет уцененных товаров
            if (!productsToOrderFrom.Any())
                return order;

            // Определяем количество видов товаров в заказе (от Min до Max)
            int productsInOrder = _random.Next(service.Config.MinProductsPerOrder,
                                             service.Config.MaxProductsPerOrder + 1);

            // Добавляем товары в заказ
            for (int i = 0; i < productsInOrder; i++)
            {
                if (!productsToOrderFrom.Any()) break;

                // Выбираем случайный товар из доступных
                var product = productsToOrderFrom[_random.Next(productsToOrderFrom.Count)];
                int packages = _random.Next(service.Config.MinPackagesPerProduct,
                                          service.Config.MaxPackagesPerProduct + 1);
                var requestedQuantity = packages * product.PackageSize;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    RequestedQuantity = requestedQuantity
                });

                // Убираем товар из списка чтобы не дублировать в одном заказе
                productsToOrderFrom.Remove(product);
            }

            return order;
        }

        public List<Order> GenerateDailyOrders(List<Store> stores, List<Product> products,
                                             IWarehouseService service, int dayOffset)
        {
            var dailyOrders = new List<Order>();

            foreach (var store in stores)
            {
                if (_random.NextDouble() < service.Config.DailyOrderProbability)
                {
                    var order = GenerateRandomOrder(store, products, service, dayOffset);
                    if (order.Items.Any())
                    {
                        dailyOrders.Add(order);
                    }
                }
            }

            return dailyOrders;
        }
    }
}