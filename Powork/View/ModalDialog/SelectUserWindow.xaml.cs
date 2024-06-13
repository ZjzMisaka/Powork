using Powork.ViewModel;
using Wpf.Ui.Controls;

namespace Powork
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class SelectUserWindow : FluentWindow
    {
        public SelectUserWindow()
        {
            InitializeComponent();
            this.DataContext = new SelectUserWindowViewModel();
        }
    }
}
