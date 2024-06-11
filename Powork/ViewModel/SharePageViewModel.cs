using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PowerThreadPool;
using Powork.Helper;
using Powork.Model;
using Powork.Network;
using Powork.Repository;
using Powork.ViewModel.Inner;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Powork.ViewModel
{
    class SharePageViewModel : ObservableObject
    {
        private User user;
        private Dictionary<string, (string, bool)> downloadingDict;
        private List<Model.FileInfo> downloadedList;
        private bool pageEnabled;
        public bool PageEnabled
        {
            get
            {
                return pageEnabled;
            }
            set
            {
                SetProperty<bool>(ref pageEnabled, value);
            }
        }
        private bool isSelf;
        public bool IsSelf
        {
            get
            {
                if (GlobalVariables.SelfInfo.Count == 0)
                {
                    return false;
                }
                return user.IP == GlobalVariables.SelfInfo[0].IP && user.Name == GlobalVariables.SelfInfo[0].Name;
            }
            set
            {
                SetProperty<bool>(ref isSelf, value);
            }
        }
        private string userName;
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                SetProperty<string>(ref userName, value);
            }
        }
        public List<ShareInfoViewModel> SelectedItems => ShareInfoList.Where(x => x.IsSelected).ToList();

        private ObservableCollection<ShareInfoViewModel> shareInfoList;
        public ObservableCollection<ShareInfoViewModel> ShareInfoList
        {
            get
            {
                return shareInfoList;
            }
            set
            {
                SetProperty<ObservableCollection<ShareInfoViewModel>>(ref shareInfoList, value);
            }
        }

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand DropCommand { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand DownloadCommand { get; set; }
        public SharePageViewModel(User user)
        {
            this.user = user;
            downloadingDict = new Dictionary<string, (string, bool)>();
            downloadedList = new List<Model.FileInfo>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);
            OpenCommand = new RelayCommand(Open);
            DownloadCommand = new RelayCommand(Download);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            if (!UserHelper.IsUserLogon())
            {
                PageEnabled = false;
            }
            if (user == null)
            {
                return;
            }

            GlobalVariables.GetShareInfo += SetShareInfo;
            GlobalVariables.GetFile += OnGetFile;

            UserName = user.Name;
            if (IsSelf)
            {
                List<ShareInfo> shareInfoList = ShareRepository.SelectFile();
                SetShareInfo(shareInfoList);
            }
            else
            {
                GlobalVariables.TcpServerClient.RequestShareInfo(user.IP, GlobalVariables.TcpPort); 
            }
        }

        private void SetShareInfo(object s, EventArgs e)
        {
            List<ShareInfo> shareInfoList = (List<ShareInfo>)s;
            SetShareInfo(shareInfoList);
        }

        private void OnGetFile(object sender, EventArgs e)
        {
            Model.FileInfo fileInfo = (Model.FileInfo)sender;
            downloadingDict.Remove(fileInfo.Guid, out (string, bool) openInfo);
            downloadedList.Add(fileInfo);
            if (downloadingDict.Count == 0)
            {
                if (openInfo.Item2)
                {
                    foreach (Model.FileInfo downloaded in downloadedList)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = openInfo.Item1,
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);
                    }
                }
                else
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"All files received successfully. Open?", "", MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        foreach (Model.FileInfo downloaded in downloadedList)
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = openInfo.Item1,
                                UseShellExecute = true
                            };
                            Process.Start(startInfo);
                        }
                    }
                }
                
                downloadedList = new List<Model.FileInfo>();
            }
        }

        private void SetShareInfo(List<ShareInfo> shareInfoList)
        {
            ShareInfoList = new ObservableCollection<ShareInfoViewModel>();
            foreach (ShareInfo shareInfo in shareInfoList)
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    ShareInfoList.Add(new ShareInfoViewModel(shareInfo));
                });
            }
        }

        private void WindowUnloaded(RoutedEventArgs eventArgs)
        {
            GlobalVariables.GetShareInfo -= SetShareInfo;
            GlobalVariables.GetFile -= OnGetFile;
        }

        private void Drop(DragEventArgs args)
        {
            string[] pathList = (string[])args.Data.GetData(DataFormats.FileDrop, false);
            foreach (string path in pathList)
            {
                if (FileHelper.GetType(path) == FileHelper.Type.Directory)
                {
                    DirectoryInfo dir = new DirectoryInfo(path);

                    ShareInfo shareInfo = new ShareInfo()
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Path = path,
                        Name = Path.GetFileNameWithoutExtension(path),
                        Extension = "",
                        Type = "Directory",
                        ShareTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        CreateTime = dir.CreationTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        LastModifiedTime = dir.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    };
                    ShareRepository.InsertFile(shareInfo);
                }
                else if (FileHelper.GetType(path) == FileHelper.Type.Image || FileHelper.GetType(path) == FileHelper.Type.File)
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);

                    ShareInfo shareInfo = new ShareInfo()
                    {
                        Guid = Guid.NewGuid().ToString(),
                        Path = path,
                        Name = Path.GetFileNameWithoutExtension(path),
                        Extension = Path.GetExtension(path),
                        Type = "File",
                        ShareTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        CreateTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        LastModifiedTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    };
                    ShareRepository.InsertFile(shareInfo);
                }
            }
            List<ShareInfo> shareInfoList = ShareRepository.SelectFile();
            ShareInfoList = new ObservableCollection<ShareInfoViewModel>();
            foreach (ShareInfo shareInfo in shareInfoList)
            {
                ShareInfoList.Add(new ShareInfoViewModel(shareInfo));
            }
        }

        private void Open()
        {
            if (IsSelf)
            {
                foreach (ShareInfoViewModel shareInfoViewModel in SelectedItems)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = shareInfoViewModel.Path,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                }
            }
            else
            {
                RequestFile(true);
            }
        }

        private void Download()
        {
            RequestFile(false);
        }

        private void RequestFile(bool tempFolder)
        {
            string directoryPath = null;
            if (tempFolder)
            {
                directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Download");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            else
            {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    fbd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        directoryPath = fbd.SelectedPath;
                    }
                }
            }
            if (directoryPath != null)
            {
                foreach (ShareInfoViewModel shareInfoViewModel in SelectedItems)
                {
                    downloadingDict.Add(shareInfoViewModel.Guid, (Path.Combine(directoryPath, shareInfoViewModel.Name + shareInfoViewModel.Extension), tempFolder));
                    GlobalVariables.TcpServerClient.RequestFile(shareInfoViewModel.Guid, user.IP, GlobalVariables.TcpPort, directoryPath);
                }
            }
        }
    }
}
