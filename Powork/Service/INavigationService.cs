using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Service
{
    internal interface INavigationService
    {
        void Navigate(Type targetType, ObservableObject dataContext);
    }
}
