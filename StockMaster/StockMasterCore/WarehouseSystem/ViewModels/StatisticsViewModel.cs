using StockMasterCore.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace WarehouseApp.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public SimulationConfig Config => Services.IntegrationService.Instance.Config;
        public WarehouseStatistics Statistics { get; set; }
        public ObservableCollection<WarehouseApp.Models.SummaryItem> StatsItems { get; set; }

        public ICommand LoadStatisticsCommand { get; }
        public ICommand RefreshCommand { get; }

        public StatisticsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            Statistics = new WarehouseStatistics();
            StatsItems = new ObservableCollection<WarehouseApp.Models.SummaryItem>();

            LoadStatisticsCommand = new Commands.RelayCommand(LoadStatistics);
            RefreshCommand = new Commands.RelayCommand(LoadStatistics);

            LoadStatistics(null);
        }

        public void LoadStatistics(object parameter)
        {
            var service = Services.IntegrationService.Instance;
            Statistics = service.WarehouseService.Statistics;
            UpdateStatsItems();

            OnPropertyChanged(nameof(Config));
        }

        private void UpdateStatsItems()
        {
            StatsItems.Clear();

            if (Statistics != null)
            {
                StatsItems.Add(new WarehouseApp.Models.SummaryItem
                {
                    Key = "Текущий день симуляции",
                    Value = Statistics.CurrentDay
                });

                StatsItems.Add(new WarehouseApp.Models.SummaryItem
                {
                    Key = "Всего продано единиц товара",
                    Value = Statistics.TotalProductsSold
                });

                StatsItems.Add(new WarehouseApp.Models.SummaryItem
                {
                    Key = "Общая выручка",
                    Value = $"{Statistics.TotalRevenue:N2} руб."
                });

                StatsItems.Add(new WarehouseApp.Models.SummaryItem
                {
                    Key = "Потери из-за уценки",
                    Value = $"{Statistics.TotalDiscountLoss:N2} руб."
                });

                StatsItems.Add(new WarehouseApp.Models.SummaryItem
                {
                    Key = "Потери из-за просрочки",
                    Value = $"{Statistics.TotalExpiredLoss:N2} руб."
                });

                StatsItems.Add(new WarehouseApp.Models.SummaryItem
                {
                    Key = "Общие убытки",
                    Value = $"{Statistics.TotalLosses:N2} руб."
                });

                StatsItems.Add(new WarehouseApp.Models.SummaryItem
                {
                    Key = "Чистая прибыль",
                    Value = $"{Statistics.NetProfit:N2} руб."
                });

                StatsItems.Add(new WarehouseApp.Models.SummaryItem
                {
                    Key = "Средние продажи в день",
                    Value = Statistics.AverageDailySales.ToString("N2")
                });
            }
        }
    }
}