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

        // Реализация интерфейса
        public List<Product> GenerateProducts(SimulationConfig config)
        {
            return GenerateProducts(config, DateTime.Now);
        }

        public List<Product> GenerateProducts(SimulationConfig config, DateTime simulationStartDate)
        {
            var products = new List<Product>();

            for (int i = 0; i < config.ProductTypesCount; i++)
            {
                int maxCapacity = _random.Next(config.MinProductCapacity, config.MaxProductCapacity + 1);
                int packageSize = _random.Next(config.MinPackageSize, config.MaxPackageSize + 1);
                int initialStock = _random.Next(maxCapacity / 6, maxCapacity / 3 + 1);

                products.Add(new Product
                {
                    Id = i + 1,
                    Name = _productNames[i % _productNames.Length],
                    Description = $"Продукт {_productNames[i % _productNames.Length]}",
                    BasePrice = _random.Next(config.MinProductPrice, config.MaxProductPrice + 1),
                    QuantityInStock = initialStock,
                    MaxCapacity = maxCapacity,
                    PackageSize = packageSize,
                    ExpiryDate = simulationStartDate.AddDays(_random.Next(config.MinExpiryDays, config.MaxExpiryDays + 1)),
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
            return GenerateRandomOrder(store, products, service, DateTime.Now.AddDays(dayOffset));
        }

        public Order GenerateRandomOrder(Store store, List<Product> products, IWarehouseService service, DateTime orderDate)
        {
            var order = new Order
            {
                Id = 0,
                StoreId = store.Id,
                OrderDate = orderDate,
                IsProcessed = false,
                TotalAmount = 0
            };

            int productsInOrder = _random.Next(1, 6);
            productsInOrder = Math.Min(productsInOrder, products.Count);

            var availableProducts = products.Where(p => p.QuantityInStock > 0).ToList();
            if (!availableProducts.Any())
                return null;

            var selectedProducts = new List<Product>();

            var discountedProducts = availableProducts.Where(p => p.IsDiscounted).ToList();
            var regularProducts = availableProducts.Where(p => !p.IsDiscounted).ToList();

            bool preferDiscounted = discountedProducts.Any() &&
                                   _random.NextDouble() < service.Config.DiscountedProductOrderProbability;

            if (preferDiscounted && discountedProducts.Any())
            {
                int discountCount = Math.Min(productsInOrder, discountedProducts.Count);
                for (int i = 0; i < discountCount; i++)
                {
                    var product = discountedProducts[_random.Next(discountedProducts.Count)];
                    if (!selectedProducts.Contains(product))
                        selectedProducts.Add(product);
                }

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
                for (int i = 0; i < productsInOrder; i++)
                {
                    var product = availableProducts[_random.Next(availableProducts.Count)];
                    if (!selectedProducts.Contains(product))
                        selectedProducts.Add(product);
                }
            }

            bool hasItems = false;

            foreach (var product in selectedProducts)
            {
                int packages = _random.Next(service.Config.MinPackagesPerProduct,
                                          service.Config.MaxPackagesPerProduct + 1);

                int requestedQuantity = packages * product.PackageSize;
                int availablePackages = product.QuantityInStock / product.PackageSize;
                int packagesToShip = Math.Min(packages, availablePackages);
                int actualQuantity = packagesToShip * product.PackageSize;

                if (actualQuantity > 0)
                {
                    hasItems = true;
                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        RequestedQuantity = requestedQuantity,
                        ActualQuantity = actualQuantity,
                        PackagesToShip = packagesToShip
                    };

                    order.Items.Add(orderItem);
                    order.TotalAmount += actualQuantity * product.CurrentPrice;
                }
            }

            return hasItems ? order : null;
        }

        public List<Order> GenerateDailyOrders(List<Store> stores, List<Product> products,
                                             IWarehouseService service, int dayOffset)
        {
            return GenerateDailyOrders(stores, products, service, DateTime.Now.AddDays(dayOffset));
        }

        public List<Order> GenerateDailyOrders(List<Store> stores, List<Product> products,
                                             IWarehouseService service, DateTime orderDate)
        {
            var dailyOrders = new List<Order>();

            foreach (var store in stores)
            {
                if (_random.NextDouble() < service.Config.DailyOrderProbability)
                {
                    var order = GenerateRandomOrder(store, products, service, orderDate);
                    if (order != null && order.Items.Any() && order.TotalAmount > 0)
                    {
                        dailyOrders.Add(order);
                    }
                }
            }

            return dailyOrders;
        }
    }
}