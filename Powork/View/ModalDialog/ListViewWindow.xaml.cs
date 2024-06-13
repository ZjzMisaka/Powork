using Powork.ViewModel;
using Wpf.Ui.Controls;

namespace Powork
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class ListViewWindow : FluentWindow
    {
        public ListViewWindow()
        {
            InitializeComponent();
            this.DataContext = new ListViewWindowViewModel();
        }
    }
}
