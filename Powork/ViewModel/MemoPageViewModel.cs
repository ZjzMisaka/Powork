using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerThreadPool;
using Powork.Model;
using Powork.Network;
using Powork.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Powork.ViewModel
{
    class MemoPageViewModel : ObservableObject
    {
        private string date;
        public string Date
        {
            get
            {
                return date;
            }
            set
            {
                SetProperty<string>(ref date, value);
                if (DateTime.TryParse(value, out DateTime dateTime))
                {
                    string formattedDate = dateTime.ToString("yyyy-MM-dd");
                    Memo = MemoRepository.SelectMemo(formattedDate);
                }
            }
        }
        private string memo;
        public string Memo
        {
            get
            {
                return memo;
            }
            set
            {
                SetProperty<string>(ref memo, value);
            }
        }
        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public MemoPageViewModel()
        {
            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            SaveCommand = new RelayCommand(Save);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            Date = DateTime.Now.ToString("yyyy-MM-dd");
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
        }

        private void Save()
        {
            MemoRepository.InsertOrUpdateMemo(Date, Memo);
        }
    }
}
