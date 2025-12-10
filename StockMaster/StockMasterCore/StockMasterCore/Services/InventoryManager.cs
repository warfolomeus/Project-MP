// Менеджер запасов. Отвечает за: управление товарными запасами, проверку сроков годности
using StockMasterCore.Models;
using StockMasterCore.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockMasterCore.Services
{
    public class InventoryManager : IInventoryManager
    {
        private readonly Random _random = new Random();

        public void CheckExpiredProducts(List<Product> products, WarehouseStatistics statistics)
        {
            var expiredProducts = products.Where(p => p.IsExpired && p.QuantityInStock > 0).ToList();

            foreach (var product in expiredProducts)
            {
                decimal loss = product.QuantityInStock * product.BasePrice;
                statistics.TotalExpiredLoss += loss;
                product.QuantityInStock = 0;
            }
        }

        public void CheckInventoryLevels(List<Product> products, List<SupplyRequest> supplyRequests, SimulationConfig config)
        {
            foreach (var product in products)
            {
                if (product.NeedsRestocking &&
                    !supplyRequests.Any(sr => sr.ProductId == product.Id && !sr.IsFulfilled))
                {
                    int quantityToOrder = product.MaxCapacity - product.QuantityInStock;
                    if (quantityToOrder > 0)
                    {
                        var supplyRequest = new SupplyRequest
                        {
                            Id = supplyRequests.Count + 1,
                            ProductId = product.Id,
                            Quantity = quantityToOrder,
                            RequestDate = DateTime.Now,
                            ExpectedDeliveryDate = DateTime.Now.AddDays(_random.Next(1, 6)),
                            IsFulfilled = false
                        };
                        supplyRequests.Add(supplyRequest);
                    }
                }
            }
        }

        public void ProcessDeliveries(List<SupplyRequest> supplyRequests, List<Product> products, SimulationConfig config)
        {
            var today = DateTime.Now;
            var deliveries = supplyRequests
                .Where(sr => !sr.IsFulfilled &&
                           sr.ExpectedDeliveryDate.HasValue &&
                           sr.ExpectedDeliveryDate.Value.Date <= today.Date)
                .ToList();

            foreach (var delivery in deliveries)
            {
                var product = products.FirstOrDefault(p => p.Id == delivery.ProductId);
                if (product != null)
                {
                    product.QuantityInStock += delivery.Quantity;
                    delivery.IsFulfilled = true;

                    // Обновляем срок годности для нового товара
                    product.ExpiryDate = DateTime.Now.AddDays(
                        _random.Next(config.MinExpiryDays, config.MaxExpiryDays + 1));

                    // Сбрасываем скидку для нового товара
                    product.DiscountPercentage = 0;
                }
            }
        }

        public int CalculatePackagesToShip(Product product, int requestedQuantity)
        {
            if (product == null || product.QuantityInStock <= 0) return 0;

            int packagesNeeded = (int)Math.Ceiling((double)requestedQuantity / product.PackageSize);
            int availablePackages = product.QuantityInStock;

            return Math.Min(packagesNeeded, availablePackages);
        }
    }
}