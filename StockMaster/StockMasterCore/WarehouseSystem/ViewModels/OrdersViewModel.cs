using StockMasterCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private Order _selectedOrder;

        public ObservableCollection<Order> TodayOrders { get; } = new ObservableCollection<Order>();
        public ObservableCollection<Order> PendingShipments { get; } = new ObservableCollection<Order>();
        public ObservableCollection<Order> AllOrders { get; } = new ObservableCollection<Order>();

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => SetField(ref _selectedOrder, value);
        }

        public ICommand LoadOrdersCommand { get; }
        public ICommand ProcessOrderCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }
        public ICommand RefreshCommand { get; }

        public OrdersViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            LoadOrdersCommand = new Commands.RelayCommand(LoadOrders);
            ProcessOrderCommand = new Commands.RelayCommand(ProcessOrder);
            ViewOrderDetailsCommand = new Commands.RelayCommand(ViewOrderDetails);
            RefreshCommand = new Commands.RelayCommand(RefreshData);

            LoadOrders(null);
        }

        private void LoadOrders(object parameter)
        {
            TodayOrders.Clear();
            PendingShipments.Clear();
            AllOrders.Clear();

            // Тестовые данные
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    StoreId = 1,
                    OrderDate = DateTime.Now,
                    TotalAmount = 1500,
                    IsProcessed = false,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { ProductId = 1, RequestedQuantity = 10, ActualQuantity = 10, PackagesToShip = 1 }
                    }
                },
                new Order
                {
                    Id = 2,
                    StoreId = 2,
                    OrderDate = DateTime.Now,
                    TotalAmount = 2400,
                    IsProcessed = true,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { ProductId = 2, RequestedQuantity = 15, ActualQuantity = 15, PackagesToShip = 2 }
                    }
                },
            };

            foreach (var order in orders)
            {
                AllOrders.Add(order);

                if (order.OrderDate.Date == DateTime.Now.Date)
                    TodayOrders.Add(order);

                if (!order.IsProcessed)
                    PendingShipments.Add(order);
            }
        }

        private void ProcessOrder(object parameter)
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Выберите заказ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedOrder.IsProcessed = true;
            MessageBox.Show($"Заказ #{SelectedOrder.Id} обработан",
                          "Заказ обработан", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshData(null);
        }

        private void ViewOrderDetails(object parameter)
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Выберите заказ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Детали заказа #{SelectedOrder.Id}\n" +
                          $"Магазин: {SelectedOrder.StoreId}\n" +
                          $"Сумма: {SelectedOrder.TotalAmount:N2} руб.\n" +
                          $"Обработан: {(SelectedOrder.IsProcessed ? "Да" : "Нет")}",
                          "Детали заказа", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshData(object parameter)
        {
            LoadOrders(null);
        }
    }
}