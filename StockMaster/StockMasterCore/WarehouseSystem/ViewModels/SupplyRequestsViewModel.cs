using StockMasterCore.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class SupplyRequestsViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public SimulationConfig Config => Services.IntegrationService.Instance.Config;
        public WarehouseStatistics Statistics => Services.IntegrationService.Instance.WarehouseService.Statistics;

        public ObservableCollection<SupplyRequest> PendingRequests { get; set; }

        public ICommand RefreshCommand { get; }
        public ICommand FulfillRequestCommand { get; }

        public SupplyRequestsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            PendingRequests = new ObservableCollection<SupplyRequest>();

            RefreshCommand = new Commands.RelayCommand(LoadRequests);
            FulfillRequestCommand = new Commands.RelayCommand(FulfillRequest);

            LoadRequests(null);
        }

        public void LoadRequests(object parameter)
        {
            var service = Services.IntegrationService.Instance;

            PendingRequests.Clear();

            try
            {
                var pendingRequests = service.GetPendingSupplyRequests();

                foreach (var request in pendingRequests)
                {
                    PendingRequests.Add(request);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnPropertyChanged(nameof(Statistics));
            OnPropertyChanged(nameof(Config));
        }

        private void FulfillRequest(object parameter)
        {
            if (parameter is SupplyRequest request)
            {
                try
                {
                    Services.IntegrationService.Instance.WarehouseService.FulfillSupplyRequest(request.Id);
                    LoadRequests(null);

                    MessageBox.Show($"Заявка #{request.Id} выполнена",
                                  "Заявка выполнена", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}