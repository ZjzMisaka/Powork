using System.Windows;
using Powork.ViewModel.ModalDialog;
using Wpf.Ui.Controls;

namespace Powork
{
    public partial class EditTaskWindow : FluentWindow
    {
        public EditTaskWindow(EditTaskWindowViewModel viewModel)
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
