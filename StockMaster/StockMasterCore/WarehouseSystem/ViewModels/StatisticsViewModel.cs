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

        public WarehouseStatistics Statistics { get; set; }
        public ObservableCollection<WarehouseApp.Models.SummaryItem> StatsItems { get; } = new ObservableCollection<WarehouseApp.Models.SummaryItem>();

        public ICommand LoadStatisticsCommand { get; }
        public ICommand ResetStatisticsCommand { get; }
        public ICommand ExportStatisticsCommand { get; }

        public StatisticsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            Statistics = new WarehouseStatistics();

            LoadStatisticsCommand = new Commands.RelayCommand(LoadStatistics);
            ResetStatisticsCommand = new Commands.RelayCommand(ResetStatistics);
            ExportStatisticsCommand = new Commands.RelayCommand(ExportStatistics);

            LoadStatistics(null);
        }

        private void LoadStatistics(object parameter)
        {
            // Тестовые данные
            Statistics.CurrentDay = 15;
            Statistics.TotalProductsSold = 1250;
            Statistics.TotalRevenue = 187500;
            Statistics.TotalDiscountLoss = 12500;
            Statistics.TotalExpiredLoss = 8500;
            Statistics.TotalInventoryValue = 450000;

            UpdateStatsItems();
        }

        private void UpdateStatsItems()
        {
            StatsItems.Clear();

            var items = new Dictionary<string, object>
            {
                ["Текущий день симуляции"] = Statistics.CurrentDay,
                ["Всего продано единиц"] = Statistics.TotalProductsSold,
                ["Общая выручка"] = $"{Statistics.TotalRevenue:N2} руб.",
                ["Потери из-за уценки"] = $"{Statistics.TotalDiscountLoss:N2} руб.",
                ["Потери из-за просрочки"] = $"{Statistics.TotalExpiredLoss:N2} руб.",
                ["Общие убытки"] = $"{Statistics.TotalLosses:N2} руб.",
                ["Стоимость остатков"] = $"{Statistics.TotalInventoryValue:N2} руб.",
                ["Чистая прибыль"] = $"{Statistics.NetProfit:N2} руб.",
                ["Средние продажи в день"] = Statistics.AverageDailySales.ToString("F2")
            };

            foreach (var item in items)
            {
                StatsItems.Add(new WarehouseApp.Models.SummaryItem { Key = item.Key, Value = item.Value });
            }
        }

        private void ResetStatistics(object parameter)
        {
            Statistics.Reset();
            UpdateStatsItems();
            MessageBox.Show("Статистика сброшена", "Сброс",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportStatistics(object parameter)
        {
            MessageBox.Show("Экспорт статистики", "В разработке",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}