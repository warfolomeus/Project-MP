//Модель заявки поставщику. Отвечает за: Запросы на пополнение запасов у поставщиков
using System;

namespace StockMasterCore.Models
{
    public class SupplyRequest
    {
        public int Id { get; set; }                          // Номер заявки
        public int ProductId { get; set; }                   // Какой товар заказываем
        public int Quantity { get; set; }                    // Сколько нужно
        public DateTime RequestDate { get; set; }            // Когда создали заявку
        public DateTime? ExpectedDeliveryDate { get; set; }  // Когда ожидаем поставку (1-5 дней)
        public bool IsFulfilled { get; set; }                // Выполнена ли поставка
    }
}