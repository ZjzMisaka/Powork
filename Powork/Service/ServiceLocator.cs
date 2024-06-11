namespace Powork.Service
{
    public static class ServiceLocator
    {
        private static IServiceProvider s_serviceProvider;

        public static void SetLocatorProvider(IServiceProvider serviceProvider)
        {
            s_serviceProvider = serviceProvider;
        }

        public static object GetService(Type serviceType)
        {
            return s_serviceProvider.GetService(serviceType);
        }

        public static T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }
    }
}
