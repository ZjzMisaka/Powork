using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Controls;

namespace Powork.Service
{
    internal class NavigationService : INavigationService
    {
        private readonly NavigationView _navigationView;
        public NavigationService(NavigationView navigationView)
        {
            _navigationView = navigationView;
        }

        public void Navigate(Type targetType, ObservableObject dataContext)
        {
            _navigationView.Navigate(targetType, dataContext);
        }
    }
}
