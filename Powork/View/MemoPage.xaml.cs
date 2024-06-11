using System.Windows.Controls;
using Powork.ViewModel;

namespace Powork.View
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class MemoPage : Page
    {
        public MemoPage()
        {
            InitializeComponent();
            this.DataContext = new MemoPageViewModel();
        }
    }
}
