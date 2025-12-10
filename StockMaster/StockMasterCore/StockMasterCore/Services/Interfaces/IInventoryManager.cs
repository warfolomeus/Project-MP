// Интерфейс менеджера запасов. Отвечает за: определение контракта для управления товарными запасами склада
using StockMasterCore.Models;
using System.Collections.Generic;

namespace StockMasterCore.Services.Interfaces
{
    public interface IInventoryManager
    {
        void CheckExpiredProducts(List<Product> products, WarehouseStatistics statistics); // Проверка и списание просрочки
        void CheckInventoryLevels(List<Product> products, List<SupplyRequest> supplyRequests, SimulationConfig config); // Проверка уровня запасов
        int CalculatePackagesToShip(Product product, int requestedQuantity); // Расчет упаковок для отгрузки
        void ProcessDeliveries(List<SupplyRequest> supplyRequests, List<Product> products, SimulationConfig config); // Обработка поставок
    }
}