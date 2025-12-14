using StockMasterCore.Models;
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
            if (Services.IntegrationService.Instance.Products.Count == 0)
            {
                MessageBox.Show("Нет данных о товарах!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CurrentView = new SupplyRequestsViewModel(this);
        }

        private void ShowStatistics()
        {
            CurrentView = new StatisticsViewModel(this);
        }

        private void ProcessDay()
        {
            if (Services.IntegrationService.Instance.Products.Count == 0)
            {
                MessageBox.Show("Нет данных для обработки!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var service = Services.IntegrationService.Instance;

            // Проверяем, не завершена ли симуляция
            if (service.IsSimulationComplete)
            {
                MessageBox.Show("Симуляция завершена! Все дни обработаны.",
                              "Симуляция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Проверяем, не превысили ли лимит дней
            if (service.CurrentDay >= service.Config.SimulationDays)
            {
                service.IsSimulationComplete = true;
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
                    MessageBox.Show($"День {service.CurrentDay} обработан. Симуляция завершена!",
                                  "Симуляция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"День {service.CurrentDay} успешно обработан. Осталось дней: {service.Config.SimulationDays - service.CurrentDay}",
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