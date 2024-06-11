using CommunityToolkit.Mvvm.ComponentModel;

namespace Powork.Service
{
    internal interface INavigationService
    {
        void Navigate(Type targetType, ObservableObject dataContext);
    }
}
