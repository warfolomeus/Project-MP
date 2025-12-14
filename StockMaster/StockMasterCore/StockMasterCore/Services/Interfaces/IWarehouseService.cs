// Интерфейс основного сервиса склада. Отвечает за: определение контракта для всех сервисов управления складом
using StockMasterCore.Models;
using System.Collections.Generic;

namespace StockMasterCore.Services.Interfaces
{
    public interface IWarehouseService
    {
        SimulationConfig Config { get; set; }                // Настройки симуляции
        WarehouseStatistics Statistics { get; }              // Статистика работы
        WarehouseSummary Summary { get; }                    // Сводная информация

        // Основные методы
        void InitializeWarehouse(List<Product> products, List<Store> stores); // Инициализация
        void ProcessDay();                                   // Обработка одного дня
        void ProcessSimulation(int days);                    // Обработка нескольких дней
        void GenerateDailyOrders(int dayOffset);            // Генерация заказов на день
        Order ProcessOrderManually(int orderId);            // Ручная обработка заказа
        void ProcessOrder(Order order);                     // Обработка заказа
        void FulfillSupplyRequest(int requestId);           // Выполнение заявки поставщика
        void ApplyDiscount(int productId, decimal discountPercentage); // Применение скидки
        void ApplyAutomaticDiscounts();                     // Автоматическое применение скидок
        void AddOrder(Order order);                         // Добавление заказа
        void AddOrders(List<Order> orders);                 // Добавление списка заказов
        void ClearProcessedOrders();                        // Очистка обработанных заказов (ДОБАВЛЕНО)

        // Методы получения данных
        List<Product> GetProducts();                        // Получение товаров
        List<Store> GetStores();                            // Получение магазинов
        List<Order> GetPendingShipments();                  // Получение готовых отгрузок
        List<SupplyRequest> GetPendingSupplyRequests();     // Получение невыполненных заявок
        List<Order> GetTodayOrders();                       // Получение заказов за день
        List<DiscountProduct> GetProductsForDiscount();     // Получение товаров для уценки
        List<Order> GetAllOrders();                         // Получение всех заказов
        List<Product> GetExpiringProducts();                // Получение товаров, скоро истекающих
        List<Product> GetLowStockProducts();                // Получение товаров с низким запасом
    }
}