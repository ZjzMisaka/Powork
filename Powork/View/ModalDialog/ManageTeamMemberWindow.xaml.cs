using Powork.ViewModel;
using Wpf.Ui.Controls;

namespace Powork
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class ManageTeamMemberWindow : FluentWindow
    {
        public ManageTeamMemberWindow()
        {
            InitializeComponent();
            this.DataContext = new ManageTeamMemberWindowViewModel();
        }
    }
}
