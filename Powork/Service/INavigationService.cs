using CommunityToolkit.Mvvm.ComponentModel;

namespace Powork.Service
{
    public interface INavigationService
    {
        void Navigate(Type targetType, ObservableObject dataContext);
    }
}
