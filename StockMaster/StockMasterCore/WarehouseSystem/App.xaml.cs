using System;
using System.Windows;

namespace WarehouseApp
{
    public partial class App : Application
    {
        public App()
        {
            // Глобальная обработка необработанных исключений
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Необработанное исключение в UI потоке:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
                          "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            MessageBox.Show($"Необработанное исключение в домене приложения:\n\n{exception?.Message}\n\n{exception?.StackTrace}",
                          "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}