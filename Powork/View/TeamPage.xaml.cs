using System.Windows.Controls;
using Powork.ViewModel;

namespace Powork.View
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class TeamPage : Page
    {
        public TeamPage()
        {
            InitializeComponent();
            this.DataContext = new TeamPageViewModel();
        }
    }
}
