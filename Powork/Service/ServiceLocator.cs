using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Service
{
    public static class ServiceLocator
    {
        private static IServiceProvider _serviceProvider;

        public static void SetLocatorProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public static T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }
    }
}
