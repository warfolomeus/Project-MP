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
            if (order == null) return order;

            order.TotalAmount = 0;

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product == null || product.QuantityInStock <= 0) continue;

                var packagesToShip = CalculatePackagesToShip(product, item.RequestedQuantity);
                if (packagesToShip == 0) continue;

                item.PackagesToShip = packagesToShip;
                item.ActualQuantity = packagesToShip * product.PackageSize;

                product.QuantityInStock -= packagesToShip;

                decimal itemTotal = item.ActualQuantity * product.CurrentPrice;
                order.TotalAmount += itemTotal;

                statistics.TotalProductsSold += item.ActualQuantity;
                statistics.TotalRevenue += itemTotal;

                if (product.DiscountPercentage > 0)
                {
                    decimal discountLoss = item.ActualQuantity * (product.BasePrice - product.CurrentPrice);
                    statistics.TotalDiscountLoss += discountLoss;
                }
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
                var processedOrder = ProcessOrder(order, products, statistics);
                processedOrders.Add(processedOrder);
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
            if (product == null) return 0;

            int packagesNeeded = (int)Math.Ceiling((double)requestedQuantity / product.PackageSize);
            int availablePackages = product.QuantityInStock;
            return Math.Min(packagesNeeded, availablePackages);
        }
    }
}