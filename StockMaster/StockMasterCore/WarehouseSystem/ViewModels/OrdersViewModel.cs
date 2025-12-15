//Управление заказами магазинов. ЗАДАЧА: Показывать заказы, поступившие за текущий день, отображать заказы, готовые к отгрузке, обрабатывать отправку заказов (вычитание запасов, обновление статистики),
//удалять отправленные заказы из списков

using StockMasterCore.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private ObservableCollection<OrderWrapper> _wrappedOrders;

        public SimulationConfig Config => Services.IntegrationService.Instance.Config;
        public WarehouseStatistics Statistics => Services.IntegrationService.Instance.WarehouseService.Statistics;

        public ObservableCollection<OrderWrapper> WrappedOrders
        {
            get => _wrappedOrders;
            set => SetField(ref _wrappedOrders, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ShipSelectedCommand { get; }

        public OrdersViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            WrappedOrders = new ObservableCollection<OrderWrapper>();
            RefreshCommand = new Commands.RelayCommand(LoadOrders);
            ShipSelectedCommand = new Commands.RelayCommand(ShipSelected);

            LoadOrders(null);
        }

        public void LoadOrders(object parameter)
        {
            try
            {
                var service = Services.IntegrationService.Instance;

                WrappedOrders.Clear();

                // Получаем все необработанные заказы
                var allOrders = service.GetAllOrders();

                // Фильтруем заказы с положительной суммой и необработанные
                var todayOrders = allOrders.Where(o =>
                    !o.IsProcessed &&
                    o.TotalAmount > 0 &&
                    o.Items.Any())
                    .ToList();

                foreach (var order in todayOrders)
                {
                    if (order != null)
                    {
                        WrappedOrders.Add(new OrderWrapper(order));
                    }
                }

                OnPropertyChanged(nameof(Statistics));
                OnPropertyChanged(nameof(Config));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShipSelected(object parameter)
        {
            try
            {
                // Создаем безопасную копию списка
                var ordersToProcess = new List<OrderWrapper>();

                foreach (var wrapper in WrappedOrders.ToList())
                {
                    if (wrapper != null && wrapper.IsSelected && !wrapper.IsProcessed)
                    {
                        ordersToProcess.Add(wrapper);
                    }
                }

                if (ordersToProcess.Count == 0)
                {
                    MessageBox.Show("Выберите заказы для отправки",
                                  "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int shippedCount = 0;
                int failedCount = 0;
                decimal totalRevenue = 0;
                var failedOrders = new List<string>();

                foreach (var wrapper in ordersToProcess)
                {
                    try
                    {
                        var processedOrder = Services.IntegrationService.Instance.WarehouseService.ProcessOrderManually(wrapper.Id);

                        if (processedOrder != null && processedOrder.IsProcessed)
                        {
                            shippedCount++;
                            totalRevenue += processedOrder.TotalAmount;
                        }
                        else
                        {
                            failedCount++;
                            failedOrders.Add($"Заказ #{wrapper.Id}: недостаточно товаров");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        failedOrders.Add($"Заказ #{wrapper.Id}: {ex.Message}");
                    }
                }

                // Обновляем данные
                LoadOrders(null);

                string message = $"Отправлено заказов: {shippedCount}\n";
                message += $"Не удалось отправить: {failedCount}\n";
                message += $"Общая выручка: {totalRevenue:N2} руб.";

                if (failedOrders.Any())
                {
                    message += $"\n\nОшибки:\n{string.Join("\n", failedOrders)}";
                    MessageBox.Show(message, "Результат отправки", MessageBoxButton.OK,
                                  shippedCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(message, "Заказы отправлены", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Обновляем статистику на UI
                OnPropertyChanged(nameof(Statistics));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadOrders(null);
            }
        }
    }

    // Класс-обертка для заказов
    public class OrderWrapper : BaseViewModel
    {
        private bool _isSelected;
        private Order _order;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public Order Order
        {
            get => _order;
            set => SetField(ref _order, value);
        }

        public int Id => Order?.Id ?? 0;
        public int StoreId => Order?.StoreId ?? 0;
        public DateTime OrderDate => Order?.OrderDate ?? DateTime.MinValue;
        public int ItemsCount => Order?.Items?.Count ?? 0;
        public decimal TotalAmount => Order?.TotalAmount ?? 0;
        public bool IsProcessed => Order?.IsProcessed ?? false;
        public string Status => IsProcessed ? "Отправлен" : "Ожидает отправки";

        public OrderWrapper(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            Order = order;
            IsSelected = false;
        }
    }
}