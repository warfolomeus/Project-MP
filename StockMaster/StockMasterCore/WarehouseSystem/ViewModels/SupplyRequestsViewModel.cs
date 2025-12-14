using StockMasterCore.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class SupplyRequestsViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private ObservableCollection<SupplyRequestWrapper> _wrappedRequests;

        public SimulationConfig Config => Services.IntegrationService.Instance.Config;
        public WarehouseStatistics Statistics => Services.IntegrationService.Instance.WarehouseService.Statistics;

        public ObservableCollection<SupplyRequestWrapper> WrappedRequests
        {
            get => _wrappedRequests;
            set => SetField(ref _wrappedRequests, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand FulfillSelectedCommand { get; }

        public SupplyRequestsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            WrappedRequests = new ObservableCollection<SupplyRequestWrapper>();
            RefreshCommand = new Commands.RelayCommand(LoadRequests);
            FulfillSelectedCommand = new Commands.RelayCommand(FulfillSelected);

            LoadRequests(null);
        }

        public void LoadRequests(object parameter)
        {
            try
            {
                var service = Services.IntegrationService.Instance;

                WrappedRequests.Clear();

                var pendingRequests = service.GetPendingSupplyRequests();

                if (pendingRequests != null)
                {
                    foreach (var request in pendingRequests)
                    {
                        if (request != null && !request.IsFulfilled)
                        {
                            WrappedRequests.Add(new SupplyRequestWrapper(request));
                        }
                    }
                }

                OnPropertyChanged(nameof(Statistics));
                OnPropertyChanged(nameof(Config));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FulfillSelected(object parameter)
        {
            try
            {
                // Создаем безопасную копию списка
                var wrappersToProcess = new List<SupplyRequestWrapper>();

                foreach (var wrapper in WrappedRequests.ToList())
                {
                    if (wrapper != null && wrapper.IsSelected && !wrapper.IsFulfilled)
                    {
                        wrappersToProcess.Add(wrapper);
                    }
                }

                if (wrappersToProcess.Count == 0)
                {
                    MessageBox.Show("Выберите заявки для выполнения",
                                  "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int fulfilledCount = 0;

                foreach (var wrapper in wrappersToProcess)
                {
                    Services.IntegrationService.Instance.WarehouseService.FulfillSupplyRequest(wrapper.Id);
                    fulfilledCount++;
                }

                // Обновляем данные
                LoadRequests(null);

                MessageBox.Show($"Выполнено {fulfilledCount} заявок",
                              "Заявки выполнены", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadRequests(null);
            }
        }
    }

    // Класс-обертка для добавления свойства выбора
    public class SupplyRequestWrapper : BaseViewModel
    {
        private bool _isSelected;
        private SupplyRequest _supplyRequest;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                // Убедитесь, что SetField вызывается и обновляет UI
                if (SetField(ref _isSelected, value))
                {
                    Console.WriteLine($"IsSelected changed to {value} for request {Id}");
                }
            }
        }

        public SupplyRequest SupplyRequest
        {
            get => _supplyRequest;
            set => SetField(ref _supplyRequest, value);
        }

        public int Id => SupplyRequest?.Id ?? 0;
        public int ProductId => SupplyRequest?.ProductId ?? 0;
        public int Quantity => SupplyRequest?.Quantity ?? 0;
        public DateTime RequestDate => SupplyRequest?.RequestDate ?? DateTime.MinValue;
        public DateTime? ExpectedDeliveryDate => SupplyRequest?.ExpectedDeliveryDate;

        public bool IsFulfilled => SupplyRequest?.IsFulfilled ?? false;
        public string Status => IsFulfilled ? "Выполнена" : "Ожидает";

        public SupplyRequestWrapper(SupplyRequest supplyRequest)
        {
            if (supplyRequest == null)
                throw new ArgumentNullException(nameof(supplyRequest));

            SupplyRequest = supplyRequest;
            IsSelected = false;
        }
    }
}