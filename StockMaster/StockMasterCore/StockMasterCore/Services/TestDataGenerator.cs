// Генератор тестовых данных для системы управления складом. Отвечает за: Создание реалистичных тестовых данных для инициализации и моделирования работы склада

using StockMasterCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockMasterCore.Services
{
    public class TestDataGenerator
    {
        // Генератор случайных чисел для создания разнообразных данных
        private readonly Random _random = new Random();

        // Справочник названий продуктов для генерации товаров
        // Содержит 15 популярных продуктовых наименований как требуется по условию (12≤ K ≤20)
        private readonly string[] _productNames =
        {
            "Рис", "Макароны", "Мука", "Сахар", "Соль",
            "Масло подсолнечное", "Чай", "Кофе", "Печенье", "Шоколад",
            "Консервы мясные", "Консервы рыбные", "Соки", "Вода", "Молоко"
        };

        // Генерация списка товаров для склада
        public List<Product> GenerateProducts(int count, int maxCapacity)
        {
            var products = new List<Product>();
            for (int i = 0; i < count; i++)
            {
                products.Add(new Product
                {
                    Id = i + 1,
                    Name = _productNames[i % _productNames.Length], // Циклическое использование имен из справочника
                    Description = $"Продукт {_productNames[i % _productNames.Length]}", // Описание на основе имени
                    BasePrice = (decimal)(_random.Next(50, 500) + _random.NextDouble()), // Случайная цена от 50 до 500 рублей
                    QuantityInStock = _random.Next(0, maxCapacity), // Случайный начальный запас от 0 до максимальной вместимости
                    MaxCapacity = maxCapacity, // Максимальное количество товара на складе
                    PackageSize = _random.Next(5, 25), // Случайный размер упаковки от 5 до 25 единиц
                    ExpiryDate = DateTime.Now.AddDays(_random.Next(1, 30)), // Случайный срок годности от 1 до 30 дней
                    ReorderThreshold = maxCapacity / 4, // Порог перезаказа = 25% от максимальной вместимости
                    DiscountPercentage = 0 // Изначально скидки нет
                });
            }

            return products;
        }

        // Генерация списка торговых точек (магазинов)
        public List<Store> GenerateStores(int count)
        {
            var stores = new List<Store>();
            // Типы торговых точек для разнообразия названий
            var storeNames = new[] { "Магазин", "Супермаркет", "Палатка", "Торговая точка" };

            for (int i = 0; i < count; i++)
            {
                stores.Add(new Store
                {
                    Id = i + 1,
                    Name = $"{storeNames[i % storeNames.Length]} {i + 1}", // Название = тип + номер
                    Address = $"ул. Урицкого, д. {i + 1}", // Условный адрес с номером
                    ContactPerson = $"Менеджер {i + 1}" // Условное контактное лицо
                });
            }

            return stores;
        }

        // Генерация случайного заказа от торговой точки
        public Order GenerateRandomOrder(List<Store> stores, List<Product> products,
                                       WarehouseService service, int dayOffset)
        {
            // Выбираем случайный магазин из списка
            var store = stores[_random.Next(stores.Count)];
            var order = new Order
            {
                StoreId = store.Id, // Привязываем заказ к выбранному магазину
                OrderDate = DateTime.Now.AddDays(dayOffset) // Устанавливаем дату заказа со смещением
            };

            // ЛОГИКА ВЫБОРА ТОВАРОВ ДЛЯ ЗАКАЗА:
            // По условию: "вероятность заказа уцененных продуктов выше, чем неуцененных"

            // Получаем список уцененных товаров
            var discountedProducts = service.GetProductsForDiscount();

            // Решаем: заказывать уцененные товары или все подряд
            // Если есть уцененные товары и случайное число меньше вероятности заказа уцененных
            var productsToOrderFrom = discountedProducts.Any() &&
                                    _random.NextDouble() < service.Config.DiscountedProductOrderProbability
                ? products.Where(p => p.DiscountPercentage > 0).ToList() // Берем только уцененные
                : products; // Иначе берем все товары

            // Если нет уцененных товаров
            if (!productsToOrderFrom.Any())
                productsToOrderFrom = products;

            // Определяем количество видов товаров в заказе (от Min до Max)
            int productsInOrder = _random.Next(service.Config.MinProductsPerOrder,
                                             service.Config.MaxProductsPerOrder + 1);

            // Добавляем товары в заказ
            for (int i = 0; i < productsInOrder; i++)
            {
                if (!productsToOrderFrom.Any()) break;

                // Выбираем случайный товар из доступных
                var product = productsToOrderFrom[_random.Next(productsToOrderFrom.Count)];

                // Определяем количество товара: случайное число упаковок, умноженное на размер упаковки
                var requestedQuantity = _random.Next(1, 10) * product.PackageSize;

                // Позицию заказа
                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id, // ID выбранного товара
                    RequestedQuantity = requestedQuantity // Запрашиваемое количество в единицах товара
                });
            }

            return order;
        }
    }
}