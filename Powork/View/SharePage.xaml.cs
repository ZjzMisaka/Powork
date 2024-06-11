using System.Windows.Controls;
using Powork.ViewModel;

namespace Powork.View
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SharePage : Page
    {
        public SharePage()
        {
            InitializeComponent();
            this.DataContext = new SharePageViewModel(GlobalVariables.SelfInfo.Count == 0 ? null : GlobalVariables.SelfInfo[0]);
        }
    }
}
