using System.Windows.Controls;
using Powork.Service;
using Powork.ViewModel;

namespace Powork.View
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.DataContext = new SettingsPageViewModel(ServiceLocator.GetService<INavigationService>());
        }
    }
}
