using StockMasterCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class SupplyRequestsViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private SupplyRequest _selectedRequest;

        public ObservableCollection<SupplyRequest> PendingRequests { get; } = new ObservableCollection<SupplyRequest>();
        public ObservableCollection<SupplyRequest> AllRequests { get; } = new ObservableCollection<SupplyRequest>();

        public SupplyRequest SelectedRequest
        {
            get => _selectedRequest;
            set => SetField(ref _selectedRequest, value);
        }

        public ICommand LoadRequestsCommand { get; }
        public ICommand FulfillRequestCommand { get; }
        public ICommand CreateRequestCommand { get; }
        public ICommand RefreshCommand { get; }

        public SupplyRequestsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            LoadRequestsCommand = new Commands.RelayCommand(LoadRequests);
            FulfillRequestCommand = new Commands.RelayCommand(FulfillRequest);
            CreateRequestCommand = new Commands.RelayCommand(CreateRequest);
            RefreshCommand = new Commands.RelayCommand(RefreshData);

            LoadRequests(null);
        }

        private void LoadRequests(object parameter)
        {
            PendingRequests.Clear();
            AllRequests.Clear();

            // Тестовые данные
            var requests = new List<SupplyRequest>
            {
                new SupplyRequest
                {
                    Id = 1,
                    ProductId = 1,
                    Quantity = 100,
                    RequestDate = DateTime.Now.AddDays(-2),
                    ExpectedDeliveryDate = DateTime.Now.AddDays(1),
                    IsFulfilled = false
                },
                new SupplyRequest
                {
                    Id = 2,
                    ProductId = 2,
                    Quantity = 80,
                    RequestDate = DateTime.Now.AddDays(-1),
                    ExpectedDeliveryDate = DateTime.Now.AddDays(2),
                    IsFulfilled = false
                },
                new SupplyRequest
                {
                    Id = 3,
                    ProductId = 3,
                    Quantity = 60,
                    RequestDate = DateTime.Now.AddDays(-3),
                    ExpectedDeliveryDate = DateTime.Now,
                    IsFulfilled = true
                },
            };

            foreach (var request in requests)
            {
                AllRequests.Add(request);

                if (!request.IsFulfilled)
                    PendingRequests.Add(request);
            }
        }

        private void FulfillRequest(object parameter)
        {
            if (SelectedRequest == null)
            {
                MessageBox.Show("Выберите заявку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedRequest.IsFulfilled = true;
            MessageBox.Show($"Заявка #{SelectedRequest.Id} выполнена",
                          "Заявка выполнена", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshData(null);
        }

        private void CreateRequest(object parameter)
        {
            MessageBox.Show("Функция создания заявки", "В разработке",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshData(object parameter)
        {
            LoadRequests(null);
        }
    }
}