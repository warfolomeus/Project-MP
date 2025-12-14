// StockMasterCore/Services/TestDataGenerator.cs
using StockMasterCore.Models;
using StockMasterCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockMasterCore.Services
{
    public class TestDataGenerator : ITestDataGenerator
    {
        private readonly Random _random = new Random();

        private readonly string[] _productNames =
        {
            "Рис", "Макароны", "Мука", "Сахар", "Соль",
            "Масло подсолнечное", "Чай", "Кофе", "Печенье", "Шоколад",
            "Консервы мясные", "Консервы рыбные", "Соки", "Вода", "Молоко"
        };

        public List<Product> GenerateProducts(SimulationConfig config)
        {
            var products = new List<Product>();

            for (int i = 0; i < config.ProductTypesCount; i++)
            {
                int maxCapacity = _random.Next(config.MinProductCapacity, config.MaxProductCapacity + 1);
                int packageSize = _random.Next(config.MinPackageSize, config.MaxPackageSize + 1);

                // Делаем начальный запас НИЗКИМ для демонстрации работы поставок
                // Вместо maxCapacity/2 используем maxCapacity/4 или меньше
                int initialStock = _random.Next(maxCapacity / 6, maxCapacity / 3 + 1);

                products.Add(new Product
                {
                    Id = i + 1,
                    Name = _productNames[i % _productNames.Length],
                    Description = $"Продукт {_productNames[i % _productNames.Length]}",
                    BasePrice = _random.Next(config.MinProductPrice, config.MaxProductPrice + 1),
                    QuantityInStock = initialStock, // МАЛЕНЬКИЙ начальный запас
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

            for (int i = 0; i < config.StoreCount; i++)
            {
                stores.Add(new Store
                {
                    Id = i + 1,
                    Name = $"Магазин №{i + 1}",
                    Address = $"ул. Торговая, д. {_random.Next(1, 100)}",
                    ContactPerson = $"Менеджер {i + 1}"
                });
            }

            return stores;
        }

        public Order GenerateRandomOrder(Store store, List<Product> products, IWarehouseService service, int dayOffset)
        {
            var order = new Order
            {
                Id = 0, // Будет установлен позже
                StoreId = store.Id,
                OrderDate = DateTime.Now.AddDays(dayOffset),
                IsProcessed = false,
                TotalAmount = 0
            };

            // Случайное количество видов товаров в заказе (1-5)
            int productsInOrder = _random.Next(1, 6);
            productsInOrder = Math.Min(productsInOrder, products.Count);

            // Выбираем случайные товары
            var availableProducts = products.Where(p => p.QuantityInStock > 0).ToList();
            if (!availableProducts.Any())
                return order;

            // Сортируем товары: сначала уцененные
            var discountedProducts = availableProducts.Where(p => p.IsDiscounted).ToList();
            var regularProducts = availableProducts.Where(p => !p.IsDiscounted).ToList();

            // Вероятность заказа уцененного товара выше
            bool preferDiscounted = discountedProducts.Any() &&
                                   _random.NextDouble() < service.Config.DiscountedProductOrderProbability;

            var selectedProducts = new List<Product>();

            if (preferDiscounted && discountedProducts.Any())
            {
                // Добавляем уцененные товары
                int discountCount = Math.Min(productsInOrder, discountedProducts.Count);
                for (int i = 0; i < discountCount; i++)
                {
                    var product = discountedProducts[_random.Next(discountedProducts.Count)];
                    if (!selectedProducts.Contains(product))
                        selectedProducts.Add(product);
                }

                // Добавляем обычные товары если нужно
                int remaining = productsInOrder - discountCount;
                if (remaining > 0 && regularProducts.Any())
                {
                    for (int i = 0; i < remaining; i++)
                    {
                        var product = regularProducts[_random.Next(regularProducts.Count)];
                        if (!selectedProducts.Contains(product))
                            selectedProducts.Add(product);
                    }
                }
            }
            else
            {
                // Добавляем случайные товары
                for (int i = 0; i < productsInOrder; i++)
                {
                    var product = availableProducts[_random.Next(availableProducts.Count)];
                    if (!selectedProducts.Contains(product))
                        selectedProducts.Add(product);
                }
            }

            // Создаем элементы заказа
            foreach (var product in selectedProducts)
            {
                // Случайное количество упаковок (1-10)
                int packages = _random.Next(service.Config.MinPackagesPerProduct,
                                          service.Config.MaxPackagesPerProduct + 1);

                int requestedQuantity = packages * product.PackageSize;

                // Рассчитываем сколько можем отгрузить
                int availablePackages = product.QuantityInStock / product.PackageSize;
                int packagesToShip = Math.Min(packages, availablePackages);
                int actualQuantity = packagesToShip * product.PackageSize;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    RequestedQuantity = requestedQuantity,
                    ActualQuantity = actualQuantity,
                    PackagesToShip = packagesToShip
                };

                order.Items.Add(orderItem);

                // Добавляем к общей сумме
                if (actualQuantity > 0)
                {
                    order.TotalAmount += actualQuantity * product.CurrentPrice;
                }
            }

            return order;
        }

        public List<Order> GenerateDailyOrders(List<Store> stores, List<Product> products,
                                             IWarehouseService service, int dayOffset)
        {
            var dailyOrders = new List<Order>();

            foreach (var store in stores)
            {
                // Магазин делает заказ с вероятностью 80%
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