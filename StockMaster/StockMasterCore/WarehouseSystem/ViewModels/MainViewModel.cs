//Главный координатор приложения. Отвечает за: управление переключением между окнами, координация работы всех ViewModelей, Обработка глобальных команд (ProcessDay, Reset)

using StockMasterCore.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set => SetField(ref _currentView, value);
        }

        public ICommand ShowSimulationSetupCommand { get; }
        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowProductsCommand { get; }
        public ICommand ShowOrdersCommand { get; }
        public ICommand ShowSupplyRequestsCommand { get; }
        public ICommand ShowStatisticsCommand { get; }
        public ICommand ProcessDayCommand { get; }
        public ICommand ResetCommand { get; }

        public MainViewModel()
        {
            ShowSimulationSetupCommand = new Commands.RelayCommand(ShowSimulationSetup);
            ShowDashboardCommand = new Commands.RelayCommand(ShowDashboard);
            ShowProductsCommand = new Commands.RelayCommand(ShowProducts);
            ShowOrdersCommand = new Commands.RelayCommand(ShowOrders);
            ShowSupplyRequestsCommand = new Commands.RelayCommand(ShowSupplyRequests);
            ShowStatisticsCommand = new Commands.RelayCommand(ShowStatistics);
            ProcessDayCommand = new Commands.RelayCommand(ProcessDay);
            ResetCommand = new Commands.RelayCommand(Reset);

            ShowSimulationSetup();
        }

        private void ShowSimulationSetup()
        {
            CurrentView = new SimulationSetupViewModel(this);
        }

        private void ShowDashboard()
        {
            if (Services.IntegrationService.Instance.Products.Count == 0)
            {
                MessageBox.Show("Сначала настройте симуляцию и сгенерируйте данные!",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                ShowSimulationSetup();
                return;
            }
            CurrentView = new WarehouseDashboardViewModel(this);
        }

        private void ShowProducts()
        {
            if (Services.IntegrationService.Instance.Products.Count == 0)
            {
                MessageBox.Show("Нет данных о товарах!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CurrentView = new ProductsViewModel(this);
        }

        private void ShowOrders()
        {
            if (Services.IntegrationService.Instance.Products.Count == 0)
            {
                MessageBox.Show("Нет данных о товарах!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CurrentView = new OrdersViewModel(this);
        }

        private void ShowSupplyRequests()
        {
            try
            {
                var service = Services.IntegrationService.Instance;

                if (service.Products.Count == 0)
                {
                    MessageBox.Show("Нет данных о товарах!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, инициализирован ли WarehouseService
                if (service.WarehouseService == null)
                {
                    MessageBox.Show("Сервис склада не инициализирован!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CurrentView = new SupplyRequestsViewModel(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия поставок: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowStatistics()
        {
            CurrentView = new StatisticsViewModel(this);
        }

        private void ProcessDay()
        {
            var service = Services.IntegrationService.Instance;

            if (service.Products.Count == 0)
            {
                MessageBox.Show("Нет данных для обработки!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, не завершена ли симуляция
            if (service.IsSimulationComplete)
            {
                MessageBox.Show("Симуляция завершена! Все дни обработаны.",
                              "Симуляция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Обрабатываем день
            bool success = service.ProcessDay();

            if (success)
            {
                // Обновляем текущий вид
                RefreshCurrentView();

                // Проверяем завершение симуляции после обработки дня
                if (service.IsSimulationComplete)
                {
                    MessageBox.Show($"День {service.CurrentDay} обработан. Симуляция завершена! Все {service.Config.SimulationDays} дней обработаны.",
                                  "Симуляция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"День {service.CurrentDay} успешно обработан. Сгенерированы новые заказы. Осталось дней: {service.Config.SimulationDays - service.CurrentDay}",
                                  "День обработан", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Симуляция завершена или произошла ошибка!",
                              "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Reset()
        {
            Services.IntegrationService.Instance.Reset();
            ShowSimulationSetup();
            MessageBox.Show("Система сброшена", "Информация",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshCurrentView()
        {
            // Обновляем текущий View
            if (CurrentView is WarehouseDashboardViewModel dashboard)
            {
                dashboard.LoadData();
            }
            else if (CurrentView is ProductsViewModel products)
            {
                products.LoadProducts(null);
            }
            else if (CurrentView is OrdersViewModel orders)
            {
                orders.LoadOrders(null);
            }
            else if (CurrentView is SupplyRequestsViewModel supply)
            {
                supply.LoadRequests(null);
            }
            else if (CurrentView is StatisticsViewModel stats)
            {
                stats.LoadStatistics(null);
            }
        }
    }
}