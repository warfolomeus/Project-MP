//Управление товарами и скидками. Отвечает за: отображение всех товаров на складе, управление скидками (ручное и автоматическое), показ товаров для уценки (скоро истекающие), обновление данных после изменений

using StockMasterCore.Models;
using System;
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
            RefreshCommand = new Commands.RelayCommand(RefreshData);
            AutoDiscountCommand = new Commands.RelayCommand(AutoDiscount);

            LoadProducts(null);
        }

        public void LoadProducts(object parameter)
        {
            try
            {
                var service = Services.IntegrationService.Instance;

                Products.Clear();
                ExpiringProducts.Clear();
                DiscountProducts.Clear();

                // Проверяем, инициализирован ли сервис
                if (service.Products == null)
                {
                    MessageBox.Show("Данные о товарах не загружены. Сначала сгенерируйте данные.",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                foreach (var product in service.Products)
                {
                    if (product == null) continue;

                    if (product.DaysUntilExpiry <= 0)
                    {
                        continue;
                    }

                    Products.Add(product);

                    // Теперь добавляем только в ExpiringProducts если срок <= 3 дней и > 0
                    if (product.DaysUntilExpiry <= 3 &&
                        product.QuantityInStock > 0)
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

                    // Добавляем в DiscountProducts только если есть скидка и срок > 0
                    if (product.IsDiscounted &&
                        product.QuantityInStock > 0)
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshData(object parameter)
        {
            var service = Services.IntegrationService.Instance;

            // Получаем актуальные продукты
            var updatedProducts = service.WarehouseService.GetProducts();
            service.Products.Clear();
            foreach (var product in updatedProducts)
                service.Products.Add(product);

            // Обновляем отображение
            LoadProducts(null);

            MessageBox.Show("Данные обновлены",
                          "Обновлено", MessageBoxButton.OK, MessageBoxImage.Information);
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