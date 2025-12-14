// Процессор заказов. Отвечает за: обработку заказов магазинов, расчет стоимостей, обновление запасов
using StockMasterCore.Models;
using StockMasterCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockMasterCore.Services
{
    public class OrderProcessor : IOrderProcessor
    {
        public Order ProcessOrder(Order order, List<Product> products, WarehouseStatistics statistics)
        {
            if (order == null || order.IsProcessed) return order;

            order.TotalAmount = 0;
            bool hasItemsToShip = false;

            // Сбрасываем ActualQuantity для пересчета
            foreach (var item in order.Items)
            {
                item.ActualQuantity = 0;
                item.PackagesToShip = 0;
            }

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null || product.QuantityInStock <= 0) continue;

                // ПЕРЕСЧИТЫВАЕМ сколько упаковок можем отгрузить СЕЙЧАС
                int packagesToShip = CalculatePackagesToShip(product, item.RequestedQuantity);
                if (packagesToShip == 0) continue;

                item.PackagesToShip = packagesToShip;
                item.ActualQuantity = packagesToShip * product.PackageSize;

                // Уменьшаем запас
                int quantityToDeduct = packagesToShip * product.PackageSize;
                product.QuantityInStock -= quantityToDeduct;

                // Рассчитываем стоимость
                decimal itemTotal = item.ActualQuantity * product.CurrentPrice;
                order.TotalAmount += itemTotal;
                hasItemsToShip = true;

                // Обновляем статистику
                statistics.TotalProductsSold += item.ActualQuantity;
                statistics.TotalRevenue += itemTotal;

                if (product.DiscountPercentage > 0)
                {
                    decimal discountLoss = item.ActualQuantity * (product.BasePrice - product.CurrentPrice);
                    statistics.TotalDiscountLoss += discountLoss;
                }
            }

            if (!hasItemsToShip)
            {
                // Заказ не может быть выполнен
                return null;
            }

            order.IsProcessed = true;

            return order;
        }

        public List<Order> ProcessDailyOrders(List<Order> orders, List<Product> products, WarehouseStatistics statistics)
        {
            var processedOrders = new List<Order>();
            var todayOrders = orders.Where(o => !o.IsProcessed).OrderBy(o => o.OrderDate).ToList();

            foreach (var order in todayOrders)
            {
                // Рассчитываем, что можем отгрузить, но не вычитаем запасы
                decimal orderTotal = 0;
                bool canBeProcessed = true;

                foreach (var item in order.Items)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null || product.QuantityInStock <= 0)
                    {
                        canBeProcessed = false;
                        continue;
                    }

                    // Проверяем, есть ли достаточно товара
                    int packagesToShip = CalculatePackagesToShip(product, item.RequestedQuantity);
                    if (packagesToShip == 0)
                    {
                        canBeProcessed = false;
                        continue;
                    }

                    item.PackagesToShip = packagesToShip;
                    item.ActualQuantity = packagesToShip * product.PackageSize;

                    // Расчет суммы, но без вычета запасов
                    decimal itemTotal = item.ActualQuantity * product.CurrentPrice;
                    orderTotal += itemTotal;
                }

                order.TotalAmount = orderTotal;
                // НЕ ставим IsProcessed = true здесь!

                if (canBeProcessed)
                {
                    processedOrders.Add(order);
                }
            }

            return processedOrders;
        }

        public decimal CalculateOrderRevenue(Order order, List<Product> products)
        {
            decimal total = 0;
            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null && item.ActualQuantity > 0)
                {
                    total += item.ActualQuantity * product.CurrentPrice;
                }
            }
            return total;
        }

        private int CalculatePackagesToShip(Product product, int requestedQuantity)
        {
            if (product == null || product.QuantityInStock <= 0) return 0;

            int packagesNeeded = (int)Math.Ceiling((double)requestedQuantity / product.PackageSize);
            int availablePackages = product.QuantityInStock / product.PackageSize;
            return Math.Min(packagesNeeded, availablePackages);
        }
    }
}