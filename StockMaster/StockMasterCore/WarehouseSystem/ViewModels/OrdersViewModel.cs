using StockMasterCore.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public SimulationConfig Config => Services.IntegrationService.Instance.Config;
        public WarehouseStatistics Statistics => Services.IntegrationService.Instance.WarehouseService.Statistics;

        public ObservableCollection<Order> TodayOrders { get; set; }
        public ObservableCollection<Order> PendingShipments { get; set; }

        public ICommand RefreshCommand { get; }
        public ICommand MarkAsShippedCommand { get; }

        public OrdersViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            TodayOrders = new ObservableCollection<Order>();
            PendingShipments = new ObservableCollection<Order>();

            RefreshCommand = new Commands.RelayCommand(LoadOrders);
            MarkAsShippedCommand = new Commands.RelayCommand(MarkAsShipped);

            LoadOrders(null);
        }

        public void LoadOrders(object parameter)
        {
            var service = Services.IntegrationService.Instance;

            TodayOrders.Clear();
            PendingShipments.Clear();

            // Получаем заказы за сегодня
            var todayOrders = service.GetTodayOrders();

            foreach (var order in todayOrders)
            {
                TodayOrders.Add(order);

                // Только необработанные заказы в готовые к отгрузке
                if (!order.IsProcessed)
                {
                    PendingShipments.Add(order);
                }
            }

            OnPropertyChanged(nameof(Statistics));
            OnPropertyChanged(nameof(Config));
        }

        private void MarkAsShipped(object parameter)
        {
            if (parameter is Order order)
            {
                try
                {
                    // Помечаем заказ как обработанный
                    order.IsProcessed = true;

                    // Обновляем отображение
                    LoadOrders(null);

                    MessageBox.Show($"Заказ #{order.Id} отмечен как отправленный",
                                  "Заказ отправлен", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}