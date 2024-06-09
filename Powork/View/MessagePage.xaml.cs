﻿using Powork.Service;
using Powork.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
