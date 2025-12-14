using System.Windows.Controls;

namespace WarehouseApp.Views
{
    public partial class SupplyRequestsWindow : UserControl
    {
        public SupplyRequestsWindow()
        {
            InitializeComponent();

            // Подписка на событие загрузки для диагностики
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                // Принудительное обновление данных при загрузке окна
                if (DataContext is ViewModels.SupplyRequestsViewModel viewModel)
                {
                    viewModel.LoadRequests(null);
                }
            }
            catch
            {
                // Игнорируем ошибки при загрузке
            }
        }
    }
}