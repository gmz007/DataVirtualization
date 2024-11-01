using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;

namespace DataVirtualization.WpfApp.Views
{
    using ViewModels;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainViewModel>();
        }

        private void ListView_TargetUpdated(object? sender, DataTransferEventArgs e)
        {
            if (sender is not ListView listView)
                return;

            if (listView.Items.Count > 0)
            {
                listView.ScrollIntoView(listView.Items[0]!);
            }
        }
    }
}