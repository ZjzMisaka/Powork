using Microsoft.Extensions.DependencyInjection;
using Powork.Service;
using Powork.ViewModel;
using Wpf.Ui.Controls;

namespace Powork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private readonly IServiceProvider _serviceProvider;
        public NavigationView NavigationView => RootNavigation;

        public MainWindow()
        {
            InitializeComponent();

            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceLocator.SetLocatorProvider(_serviceProvider);

            this.DataContext = new MainWindowViewModel(ServiceLocator.GetService<INavigationService>());

            notifyIcon.Menu.DataContext = this.DataContext;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<MainWindow>();

            services.AddSingleton<NavigationView>(provider =>
            {
                return NavigationView;
            });

            services.AddSingleton<INavigationService, Service.NavigationService>(provider =>
            {
                NavigationView navigationView = provider.GetRequiredService<NavigationView>();
                return new Service.NavigationService(navigationView);
            });
        }
    }
}
