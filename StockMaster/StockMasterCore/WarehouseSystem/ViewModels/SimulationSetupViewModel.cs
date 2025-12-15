//Настройка симуляции. Позволяет пользователю настроить параметры симуляции, генерировать тестовые данные (товары, магазины), запускать основную работу системы


using StockMasterCore.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class SimulationSetupViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public SimulationConfig Config
        {
            get => Services.IntegrationService.Instance.Config;
            set => Services.IntegrationService.Instance.Config = value;
        }

        public WarehouseStatistics Statistics => Services.IntegrationService.Instance.WarehouseService.Statistics;

        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<Store> Stores { get; set; }

        public ICommand GenerateDataCommand { get; }
        public ICommand StartSimulationCommand { get; }
        public ICommand ShowDashboardCommand { get; }
        public ICommand ResetCommand { get; }

        public SimulationSetupViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            Products = new ObservableCollection<Product>();
            Stores = new ObservableCollection<Store>();

            GenerateDataCommand = new Commands.RelayCommand(GenerateTestData);
            StartSimulationCommand = new Commands.RelayCommand(StartSimulation);
            ShowDashboardCommand = new Commands.RelayCommand(ShowDashboard);
            ResetCommand = new Commands.RelayCommand(Reset);

            LoadExistingData();
        }

        private void LoadExistingData()
        {
            Products.Clear();
            Stores.Clear();

            var service = Services.IntegrationService.Instance;

            foreach (var product in service.Products)
                Products.Add(product);

            foreach (var store in service.Stores)
                Stores.Add(store);
        }

        private void GenerateTestData(object parameter)
        {
            Services.IntegrationService.Instance.GenerateTestData();
            LoadExistingData();

            MessageBox.Show($"Сгенерировано {Products.Count} товаров и {Stores.Count} магазинов",
                          "Данные созданы", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StartSimulation(object parameter)
        {
            if (!Products.Any())
            {
                MessageBox.Show("Сначала сгенерируйте данные", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show("Симуляция начата! Переходим к панели управления.",
                          "Симуляция", MessageBoxButton.OK, MessageBoxImage.Information);

            ShowDashboard();
        }

        private void ShowDashboard()
        {
            _mainViewModel.ShowDashboardCommand.Execute(null);
        }

        private void Reset(object parameter)
        {
            _mainViewModel.ResetCommand.Execute(null);
        }
    }
}