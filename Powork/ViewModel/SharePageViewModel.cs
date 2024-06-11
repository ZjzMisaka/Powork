using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Powork.Constant;
using Powork.Helper;
using Powork.Model;
using Powork.Repository;
using Powork.ViewModel.Inner;

namespace Powork.ViewModel
{
    class SharePageViewModel : ObservableObject
    {
        private readonly User _user;
        private readonly Dictionary<string, (string, bool)> _downloadingDict;
        private List<string> _downloadedList;
        private bool _pageEnabled;
        public bool PageEnabled
        {
            get
            {
                return _pageEnabled;
            }
            set
            {
                SetProperty<bool>(ref _pageEnabled, value);
            }
        }
        private bool _isSelf;
        public bool IsSelf
        {
            get
            {
                if (GlobalVariables.SelfInfo.Count == 0)
                {
                    return false;
                }
                return _user.IP == GlobalVariables.SelfInfo[0].IP && _user.Name == GlobalVariables.SelfInfo[0].Name;
            }
            set
            {
                SetProperty<bool>(ref _isSelf, value);
            }
        }
        private string _userName;
        public string UserName
        {
            get
            {
                return _userName + "'s Sharing";
            }
            set
            {
                SetProperty<string>(ref _userName, value);
            }
        }
        public List<ShareInfoViewModel> SelectedItems => ShareInfoList.Where(x => x.IsSelected).ToList();

        private ObservableCollection<ShareInfoViewModel> _shareInfoList;
        public ObservableCollection<ShareInfoViewModel> ShareInfoList
        {
            get
            {
                return _shareInfoList;
            }
            set
            {
                SetProperty<ObservableCollection<ShareInfoViewModel>>(ref _shareInfoList, value);
            }
        }

        public ICommand WindowLoadedCommand { get; set; }
        public ICommand WindowUnloadedCommand { get; set; }
        public ICommand DropCommand { get; set; }
        public ICommand OpenCommand { get; set; }
        public ICommand DownloadCommand { get; set; }
        public ICommand RemoveCommand { get; set; }
        public SharePageViewModel(User user)
        {
            this._user = user;
            _downloadingDict = new Dictionary<string, (string, bool)>();
            _downloadedList = new List<string>();

            WindowLoadedCommand = new RelayCommand<RoutedEventArgs>(WindowLoaded);
            WindowUnloadedCommand = new RelayCommand<RoutedEventArgs>(WindowUnloaded);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);
            OpenCommand = new RelayCommand(Open);
            DownloadCommand = new RelayCommand(Download);
            RemoveCommand = new RelayCommand(Remove);
        }

        private void WindowLoaded(RoutedEventArgs eventArgs)
        {
            if (!UserHelper.IsUserLogon())
            {
                PageEnabled = false;
            }
            if (_user == null)
            {
                return;
            }

            GlobalVariables.GetShareInfo += SetShareInfo;
            GlobalVariables.GetFile += OnGetFile;

            UserName = _user.Name;
            if (IsSelf)
            {
                List<ShareInfo> shareInfoList = ShareRepository.SelectFile();
                SetShareInfo(shareInfoList);
            }
            else
            {
                GlobalVariables.TcpServerClient.RequestShareInfo(_user.IP, GlobalVariables.TcpPort);
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
            _downloadingDict.Remove(fileInfo.Guid, out (string, bool) openInfo);
            _downloadedList.Add(openInfo.Item1);
            if (_downloadingDict.Count == 0)
            {
                if (openInfo.Item2)
                {
                    foreach (string path in _downloadedList)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = path,
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);
                    }
                }
                else
                {
                    MessageBoxResult messageBoxResult = MessageBox.Show($"All files received successfully. Open?", string.Empty, MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        foreach (string path in _downloadedList)
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = path,
                                UseShellExecute = true
                            };
                            Process.Start(startInfo);
                        }
                    }
                }

                _downloadedList = new List<string>();
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
                        Extension = string.Empty,
                        Type = "Directory",
                        Size = string.Empty,
                        ShareTime = DateTime.Now.ToString(Format.DateTimeFormatWithMilliseconds),
                        CreateTime = dir.CreationTime.ToString(Format.DateTimeFormatWithMilliseconds),
                        LastModifiedTime = dir.LastWriteTime.ToString(Format.DateTimeFormatWithMilliseconds),
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
                        Size = FileHelper.GetReadableFileSize(fileInfo.Length),
                        ShareTime = DateTime.Now.ToString(Format.DateTimeFormatWithMilliseconds),
                        CreateTime = fileInfo.CreationTime.ToString(Format.DateTimeFormatWithMilliseconds),
                        LastModifiedTime = fileInfo.LastWriteTime.ToString(Format.DateTimeFormatWithMilliseconds),
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

        private void Remove()
        {
            if (IsSelf)
            {
                foreach (ShareInfoViewModel shareInfoViewModel in SelectedItems)
                {
                    ShareRepository.RemoveFile(shareInfoViewModel.Guid);
                    ShareInfoList.Remove(shareInfoViewModel);
                }
            }
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
                    _downloadingDict.Add(shareInfoViewModel.Guid, (Path.Combine(directoryPath, shareInfoViewModel.Name + shareInfoViewModel.Extension), tempFolder));
                    GlobalVariables.TcpServerClient.RequestFile(shareInfoViewModel.Guid, _user.IP, GlobalVariables.TcpPort, directoryPath);
                }
            }
        }
    }
}
