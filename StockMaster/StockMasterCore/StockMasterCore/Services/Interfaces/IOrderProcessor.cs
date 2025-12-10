// Интерфейс процессора заказов. Отвечает за: определение контракта для обработки заказов магазинов
using StockMasterCore.Models;
using System.Collections.Generic;

namespace StockMasterCore.Services.Interfaces
{
    public interface IOrderProcessor
    {
        Order ProcessOrder(Order order, List<Product> products, WarehouseStatistics statistics); // Обработка заказа
        decimal CalculateOrderRevenue(Order order, List<Product> products); // Расчет выручки
        List<Order> ProcessDailyOrders(List<Order> orders, List<Product> products, WarehouseStatistics statistics); // Обработка всех заказов дня
    }
}