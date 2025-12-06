using CommunityToolkit.Mvvm.ComponentModel;

namespace Powork.ViewModel.ModalDialog
{
    public partial class AddProjectWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;
    }
}
