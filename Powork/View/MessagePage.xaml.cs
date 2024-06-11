using System.Windows.Controls;
using Powork.Service;
using Powork.ViewModel;

namespace Powork.View
{
    /// <summary>
    /// MessagePage.xaml 的交互逻辑
    /// </summary>
    public partial class MessagePage : Page
    {
        public MessagePage()
        {
            InitializeComponent();
            this.DataContext = new MessagePageViewModel(ServiceLocator.GetService<INavigationService>());
        }
    }
}
