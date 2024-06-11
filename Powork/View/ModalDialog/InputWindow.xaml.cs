using Powork.ViewModel;
using Wpf.Ui.Controls;

namespace Powork
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : FluentWindow
    {
        public InputWindow()
        {
            InitializeComponent();
            this.DataContext = new InputWindowViewModel();
        }
    }
}
