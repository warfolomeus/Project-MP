using StockMasterCore.Models;
using StockMasterCore.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class SimulationSetupViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private SimulationConfig _config;

        public SimulationConfig Config
        {
            get => _config;
            set => SetField(ref _config, value);
        }

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Store> Stores { get; } = new ObservableCollection<Store>();

        public ICommand GenerateDataCommand { get; }
        public ICommand StartSimulationCommand { get; }
        public ICommand ShowDashboardCommand { get; }

        public SimulationSetupViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            Config = new SimulationConfig();

            GenerateDataCommand = new Commands.RelayCommand(GenerateTestData);
            StartSimulationCommand = new Commands.RelayCommand(StartSimulation);
            ShowDashboardCommand = new Commands.RelayCommand(() => _mainViewModel.ShowDashboardCommand.Execute(null));
        }

        private void GenerateTestData(object parameter)
        {
            var generator = WarehouseServiceFactory.CreateTestDataGenerator();
            var products = generator.GenerateProducts(Config);
            var stores = generator.GenerateStores(Config);

            Products.Clear();
            foreach (var product in products)
                Products.Add(product);

            Stores.Clear();
            foreach (var store in stores)
                Stores.Add(store);

            MessageBox.Show($"Сгенерировано {products.Count} товаров и {stores.Count} магазинов",
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

            _mainViewModel.ShowDashboardCommand.Execute(null);
        }
    }
}