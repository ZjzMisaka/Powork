using System.Windows;
using Powork.ViewModel.ModalDialog;
using Wpf.Ui.Controls;

namespace Powork
{
    public partial class AddProjectWindow : FluentWindow
    {
        public AddProjectWindow(AddProjectWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
