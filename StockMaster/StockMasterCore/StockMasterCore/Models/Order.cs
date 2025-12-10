// Модель заказа. Отвечает за: Представление заказов от магазинов и их содержимого
using System;
using System.Collections.Generic;

namespace StockMasterCore.Models
{
    public class Order
    {
        public int Id { get; set; }                          // Номер заказа
        public int StoreId { get; set; }                     // Какой магазин заказал
        public DateTime OrderDate { get; set; }              // Дата заказа
        public List<OrderItem> Items { get; set; } = new List<OrderItem>(); // Список товаров в заказе
        public bool IsProcessed { get; set; }                // Обработан ли заказ
        public decimal TotalAmount { get; set; }             // Общая сумма
    }

    public class OrderItem
    {
        public int ProductId { get; set; }                   // ID товара
        public int RequestedQuantity { get; set; }           // Сколько хочет магазин (в штуках)
        public int ActualQuantity { get; set; }              // Сколько фактически отгрузили
        public int PackagesToShip { get; set; }              // Сколько упаковок отгружаем
    }
}