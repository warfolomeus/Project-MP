using StockMasterCore.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class WarehouseDashboardViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public SimulationConfig Config => Services.IntegrationService.Instance.Config;
        public WarehouseStatistics Statistics => Services.IntegrationService.Instance.WarehouseService.Statistics;

        public WarehouseSummary Summary { get; set; }
        public ObservableCollection<Product> LowStockProducts { get; set; }
        public ObservableCollection<WarehouseApp.Models.ProductDisplay> ExpiringProducts { get; set; }
        public ObservableCollection<WarehouseApp.Models.SummaryItem> SummaryItems { get; set; }

        public ICommand RefreshCommand { get; }
        public ICommand ShowProductsCommand { get; }
        public ICommand ShowOrdersCommand { get; }

        public WarehouseDashboardViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            Summary = new WarehouseSummary();
            LowStockProducts = new ObservableCollection<Product>();
            ExpiringProducts = new ObservableCollection<WarehouseApp.Models.ProductDisplay>();
            SummaryItems = new ObservableCollection<WarehouseApp.Models.SummaryItem>();

            RefreshCommand = new Commands.RelayCommand(LoadData);
            ShowProductsCommand = new Commands.RelayCommand(() =>
                _mainViewModel.ShowProductsCommand.Execute(null));
            ShowOrdersCommand = new Commands.RelayCommand(() =>
                _mainViewModel.ShowOrdersCommand.Execute(null));

            LoadData();
        }

        public void LoadData(object parameter = null)
        {
            var service = Services.IntegrationService.Instance;

            // Обновляем сводку
            Summary = service.WarehouseService.Summary;
            UpdateSummaryItems();

            // Обновляем товары с низким запасом
            LowStockProducts.Clear();
            var lowStock = service.WarehouseService.GetLowStockProducts();
            foreach (var product in lowStock)
            {
                // Пропускаем товары с 0 дней до истечения
                if (product.DaysUntilExpiry > 0)
                {
                    LowStockProducts.Add(product);
                }
            }

            // Обновляем товары для уценки
            ExpiringProducts.Clear();
            var expiring = service.WarehouseService.GetExpiringProducts();
            foreach (var product in expiring)
            {
                // Пропускаем товары с 0 дней до истечения
                if (product.DaysUntilExpiry > 0)
                {
                    ExpiringProducts.Add(new WarehouseApp.Models.ProductDisplay
                    {
                        Id = product.Id,
                        Name = product.Name,
                        DaysUntilExpiry = product.DaysUntilExpiry,
                        DiscountPercentage = product.DiscountPercentage,
                        CurrentPrice = product.CurrentPrice,
                        QuantityInStock = product.QuantityInStock
                    });
                }
            }

            OnPropertyChanged(nameof(Summary));
            OnPropertyChanged(nameof(Statistics));
            OnPropertyChanged(nameof(Config));
        }

        private void UpdateSummaryItems()
        {
            SummaryItems.Clear();

            if (Summary != null)
            {
                SummaryItems.Add(new WarehouseApp.Models.SummaryItem { Key = "Всего товаров", Value = Summary.TotalProducts });
                SummaryItems.Add(new WarehouseApp.Models.SummaryItem { Key = "Магазинов", Value = Summary.TotalStores });
                SummaryItems.Add(new WarehouseApp.Models.SummaryItem { Key = "Всего заказов", Value = Summary.TotalOrders });
                SummaryItems.Add(new WarehouseApp.Models.SummaryItem { Key = "Товаров в наличии", Value = Summary.ActiveProducts });
                SummaryItems.Add(new WarehouseApp.Models.SummaryItem { Key = "С низким запасом", Value = Summary.LowStockProducts });
                SummaryItems.Add(new WarehouseApp.Models.SummaryItem { Key = "Скоро истекают", Value = Summary.ExpiringSoonProducts });
                SummaryItems.Add(new WarehouseApp.Models.SummaryItem { Key = "Готово к отгрузке", Value = Summary.PendingShipments });
                SummaryItems.Add(new WarehouseApp.Models.SummaryItem { Key = "Ожидают поставки", Value = Summary.PendingSupplyRequests });
            }
        }
    }
}