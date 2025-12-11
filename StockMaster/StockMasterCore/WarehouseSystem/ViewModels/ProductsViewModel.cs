using StockMasterCore.Models;
using System.Collections.Generic;
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

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<WarehouseApp.Models.ProductDisplay> ExpiringProducts { get; } = new ObservableCollection<WarehouseApp.Models.ProductDisplay>();
        public ObservableCollection<DiscountProduct> DiscountProducts { get; } = new ObservableCollection<DiscountProduct>();

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetField(ref _selectedProduct, value);
        }

        public decimal DiscountPercentage { get; set; }

        public ICommand LoadProductsCommand { get; }
        public ICommand ApplyDiscountCommand { get; }
        public ICommand RemoveDiscountCommand { get; }
        public ICommand RefreshCommand { get; }

        public ProductsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            LoadProductsCommand = new Commands.RelayCommand(LoadProducts);
            ApplyDiscountCommand = new Commands.RelayCommand(ApplyDiscount);
            RemoveDiscountCommand = new Commands.RelayCommand(RemoveDiscount);
            RefreshCommand = new Commands.RelayCommand(RefreshData);

            LoadProducts(null);
        }

        private void LoadProducts(object parameter)
        {
            Products.Clear();
            ExpiringProducts.Clear();
            DiscountProducts.Clear();

            // Тестовые данные
            var product1 = new Product
            {
                Id = 1,
                Name = "Рис",
                BasePrice = 100,
                QuantityInStock = 50,
                MaxCapacity = 200,
                PackageSize = 10
            };
            product1.ExpiryDate = System.DateTime.Now.AddDays(30);

            var product2 = new Product
            {
                Id = 2,
                Name = "Макароны",
                BasePrice = 80,
                QuantityInStock = 30,
                MaxCapacity = 150,
                PackageSize = 8
            };
            product2.ExpiryDate = System.DateTime.Now.AddDays(25);

            var product3 = new Product
            {
                Id = 3,
                Name = "Молоко",
                BasePrice = 70,
                QuantityInStock = 20,
                MaxCapacity = 100,
                PackageSize = 6,
                DiscountPercentage = 20
            };
            product3.ExpiryDate = System.DateTime.Now.AddDays(2);

            var product4 = new Product
            {
                Id = 4,
                Name = "Хлеб",
                BasePrice = 50,
                QuantityInStock = 15,
                MaxCapacity = 80,
                PackageSize = 5,
                DiscountPercentage = 30
            };
            product4.ExpiryDate = System.DateTime.Now.AddDays(1);

            var products = new List<Product> { product1, product2, product3, product4 };

            foreach (var product in products)
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

            SelectedProduct.DiscountPercentage = DiscountPercentage;
            MessageBox.Show($"Скидка {DiscountPercentage}% применена к товару {SelectedProduct.Name}",
                          "Скидка применена", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshData(null);
        }

        private void RemoveDiscount(object parameter)
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Выберите товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedProduct.DiscountPercentage = 0;
            MessageBox.Show($"Скидка снята с товара {SelectedProduct.Name}",
                          "Скидка снята", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshData(null);
        }

        private void RefreshData(object parameter)
        {
            LoadProducts(null);
        }
    }
}