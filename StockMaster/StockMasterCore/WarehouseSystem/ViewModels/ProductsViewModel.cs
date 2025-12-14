using StockMasterCore.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private Product _selectedProduct;
        private decimal _discountPercentage;

        public SimulationConfig Config => Services.IntegrationService.Instance.Config;
        public WarehouseStatistics Statistics => Services.IntegrationService.Instance.WarehouseService.Statistics;

        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<WarehouseApp.Models.ProductDisplay> ExpiringProducts { get; set; }
        public ObservableCollection<DiscountProduct> DiscountProducts { get; set; }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetField(ref _selectedProduct, value);
        }

        public decimal DiscountPercentage
        {
            get => _discountPercentage;
            set => SetField(ref _discountPercentage, value);
        }

        public ICommand LoadProductsCommand { get; }
        public ICommand ApplyDiscountCommand { get; }
        public ICommand RemoveDiscountCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AutoDiscountCommand { get; }

        public ProductsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            Products = new ObservableCollection<Product>();
            ExpiringProducts = new ObservableCollection<WarehouseApp.Models.ProductDisplay>();
            DiscountProducts = new ObservableCollection<DiscountProduct>();

            LoadProductsCommand = new Commands.RelayCommand(LoadProducts);
            ApplyDiscountCommand = new Commands.RelayCommand(ApplyDiscount);
            RemoveDiscountCommand = new Commands.RelayCommand(RemoveDiscount);
            RefreshCommand = new Commands.RelayCommand(LoadProducts);
            AutoDiscountCommand = new Commands.RelayCommand(AutoDiscount);

            LoadProducts(null);
        }

        public void LoadProducts(object parameter)
        {
            var service = Services.IntegrationService.Instance;

            Products.Clear();
            ExpiringProducts.Clear();
            DiscountProducts.Clear();

            foreach (var product in service.Products)
            {
                Products.Add(product);

                if (product.DaysUntilExpiry <= 3 && product.DaysUntilExpiry > 0)
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

                if (product.IsDiscounted)
                {
                    DiscountProducts.Add(new DiscountProduct
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        OriginalPrice = product.BasePrice,
                        DiscountedPrice = product.CurrentPrice,
                        DiscountPercentage = product.DiscountPercentage,
                        DaysUntilExpiry = product.DaysUntilExpiry,
                        CurrentStock = product.QuantityInStock
                    });
                }
            }

            OnPropertyChanged(nameof(Statistics));
            OnPropertyChanged(nameof(Config));
        }

        private void ApplyDiscount(object parameter)
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Выберите товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DiscountPercentage <= 0 || DiscountPercentage > 100)
            {
                MessageBox.Show("Введите корректный процент скидки (1-100)", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Services.IntegrationService.Instance.ApplyDiscount(SelectedProduct.Id, DiscountPercentage);
            MessageBox.Show($"Скидка {DiscountPercentage}% применена к товару {SelectedProduct.Name}",
                          "Скидка применена", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadProducts(null);
        }

        private void RemoveDiscount(object parameter)
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Выберите товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Services.IntegrationService.Instance.RemoveDiscount(SelectedProduct.Id);
            MessageBox.Show($"Скидка снята с товара {SelectedProduct.Name}",
                          "Скидка снята", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadProducts(null);
        }

        private void AutoDiscount(object parameter)
        {
            Services.IntegrationService.Instance.WarehouseService.ApplyAutomaticDiscounts();
            LoadProducts(null);
            MessageBox.Show("Автоматические скидки применены",
                          "Скидки", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}