using StockMasterCore.Services;
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
        public ICommand RunFullSimulationCommand { get; }

        public MainViewModel()
        {
            // Инициализация команд
            ShowSimulationSetupCommand = new Commands.RelayCommand(ShowSimulationSetup);
            ShowDashboardCommand = new Commands.RelayCommand(ShowDashboard);
            ShowProductsCommand = new Commands.RelayCommand(ShowProducts);
            ShowOrdersCommand = new Commands.RelayCommand(ShowOrders);
            ShowSupplyRequestsCommand = new Commands.RelayCommand(ShowSupplyRequests);
            ShowStatisticsCommand = new Commands.RelayCommand(ShowStatistics);
            ProcessDayCommand = new Commands.RelayCommand(ProcessDay);
            RunFullSimulationCommand = new Commands.RelayCommand(RunFullSimulation);

            // Показываем окно настройки по умолчанию
            ShowSimulationSetup();
        }

        private void ShowSimulationSetup()
        {
            CurrentView = new SimulationSetupViewModel(this);
        }

        private void ShowDashboard()
        {
            CurrentView = new WarehouseDashboardViewModel(this);
        }

        private void ShowProducts()
        {
            CurrentView = new ProductsViewModel(this);
        }

        private void ShowOrders()
        {
            CurrentView = new OrdersViewModel(this);
        }

        private void ShowSupplyRequests()
        {
            CurrentView = new SupplyRequestsViewModel(this);
        }

        private void ShowStatistics()
        {
            CurrentView = new StatisticsViewModel(this);
        }

        private void ProcessDay()
        {
            var warehouseService = WarehouseServiceFactory.CreateWarehouseService();
            // Здесь должна быть логика обработки дня
            MessageBox.Show("День обработан", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RunFullSimulation()
        {
            MessageBox.Show("Запуск полной симуляции", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}