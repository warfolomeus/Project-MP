using StockMasterCore.Models;
using StockMasterCore.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class WarehouseDashboardViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private WarehouseSummary _summary;

        public WarehouseSummary Summary
        {
            get => _summary;
            set => SetField(ref _summary, value);
        }

        public ObservableCollection<Product> LowStockProducts { get; } = new ObservableCollection<Product>();
        public ObservableCollection<WarehouseApp.Models.ProductDisplay> ExpiringProducts { get; } = new ObservableCollection<WarehouseApp.Models.ProductDisplay>();
        public ObservableCollection<WarehouseApp.Models.SummaryItem> SummaryItems { get; } = new ObservableCollection<WarehouseApp.Models.SummaryItem>();

        public ICommand RefreshCommand { get; }
        public ICommand ShowProductsCommand { get; }
        public ICommand ShowOrdersCommand { get; }

        public WarehouseDashboardViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            Summary = new WarehouseSummary();

            RefreshCommand = new Commands.RelayCommand(RefreshData);
            ShowProductsCommand = new Commands.RelayCommand(() => _mainViewModel.ShowProductsCommand.Execute(null));
            ShowOrdersCommand = new Commands.RelayCommand(() => _mainViewModel.ShowOrdersCommand.Execute(null));

            RefreshData(null);
        }

        private void RefreshData(object parameter)
        {
            // Здесь должна быть логика получения данных из сервиса
            // Временно создаем тестовые данные

            var warehouseService = WarehouseServiceFactory.CreateWarehouseService();

            // Тестовые данные для демонстрации
            Summary.TotalProducts = 15;
            Summary.TotalStores = 5;
            Summary.TotalOrders = 42;
            Summary.ActiveProducts = 12;
            Summary.LowStockProducts = 3;
            Summary.ExpiringSoonProducts = 2;
            Summary.PendingShipments = 5;
            Summary.PendingSupplyRequests = 3;

            UpdateSummaryItems();

            // Тестовые товары с низким запасом
            LowStockProducts.Clear();
            LowStockProducts.Add(new Product { Id = 1, Name = "Рис", QuantityInStock = 5, ReorderThreshold = 10 });
            LowStockProducts.Add(new Product { Id = 2, Name = "Макароны", QuantityInStock = 3, ReorderThreshold = 8 });

            // Тестовые товары для уценки (используем ProductDisplay вместо Product)
            ExpiringProducts.Clear();
            ExpiringProducts.Add(new WarehouseApp.Models.ProductDisplay
            {
                Id = 3,
                Name = "Молоко",
                DaysUntilExpiry = 2,
                DiscountPercentage = 20
            });
            ExpiringProducts.Add(new WarehouseApp.Models.ProductDisplay
            {
                Id = 4,
                Name = "Хлеб",
                DaysUntilExpiry = 1,
                DiscountPercentage = 30
            });
        }

        private void UpdateSummaryItems()
        {
            SummaryItems.Clear();

            var items = new Dictionary<string, object>
            {
                ["Всего товаров"] = Summary.TotalProducts,
                ["Магазинов"] = Summary.TotalStores,
                ["Всего заказов"] = Summary.TotalOrders,
                ["Товаров в наличии"] = Summary.ActiveProducts,
                ["С низким запасом"] = Summary.LowStockProducts,
                ["Скоро истекают"] = Summary.ExpiringSoonProducts,
                ["Готово к отгрузке"] = Summary.PendingShipments,
                ["Ожидают поставки"] = Summary.PendingSupplyRequests
            };

            foreach (var item in items)
            {
                SummaryItems.Add(new WarehouseApp.Models.SummaryItem { Key = item.Key, Value = item.Value });
            }
        }
    }
}