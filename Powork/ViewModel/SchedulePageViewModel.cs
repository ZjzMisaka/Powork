using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Powork.Model;
using Powork.Repository;
using Powork.ViewModel.Inner;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Task = Powork.Model.Task;

namespace Powork.ViewModel
{
    public partial class SchedulePageViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime _selectedMonth;

        [ObservableProperty]
        private ObservableCollection<User> _users;

        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    RequestScheduleForUser();
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<Project> _projects;

        private Project _selectedProject;
        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (SetProperty(ref _selectedProject, value))
                {
                    GenerateSchedule();
                }
            }
        }


        [ObservableProperty]
        private ObservableCollection<ScheduleTaskViewModel> _scheduleTasks;

        private List<Holiday> _holidays;

        public int DaysInMonth => DateTime.DaysInMonth(SelectedMonth.Year, SelectedMonth.Month);
        public event EventHandler GenerateColumns;

        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand AddProjectCommand { get; }
        public ICommand ManageProjectCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddTaskCommand { get; private set; }
        public ICommand EditTaskCommand { get; private set; }
        public ICommand DeleteTaskCommand { get; private set; }
        public ICommand SetAsHolidayCommand { get; private set; }
        public ICommand SetAsWorkdayCommand { get; private set; }

        public SchedulePageViewModel()
        {
            SelectedMonth = DateTime.Now;
            Users = GlobalVariables.UserList;
            
            LoadProjects();

            PreviousMonthCommand = new RelayCommand(() => { SelectedMonth = SelectedMonth.AddMonths(-1); });
            NextMonthCommand = new RelayCommand(() => { SelectedMonth = SelectedMonth.AddMonths(1); });
            AddProjectCommand = new RelayCommand(AddProject);
            ManageProjectCommand = new RelayCommand(ManageProject);
            RefreshCommand = new RelayCommand(Refresh);

            InitializeCommands();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectedMonth))
                {
                    GenerateSchedule();
                }
            };

            if (!Projects.Any())
            {
                AddDummyData();
                LoadProjects();
            }
            if (Projects.Any())
            {
                SelectedProject = Projects.First();
            }
            SelectedUser = GlobalVariables.SelfInfo;

            GlobalVariables.ProjectListReceived += OnProjectListReceived;
            GlobalVariables.ScheduleReceived += OnScheduleReceived;
        }

        private void OnScheduleReceived(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                GenerateSchedule();
            });
        }

        private void RequestScheduleForUser()
        {
            if (SelectedUser == null || SelectedProject == null || SelectedUser.IP == GlobalVariables.SelfInfo.IP)
            {
                GenerateSchedule();
                return;
            }

            var request = new
            {
                ProjectId = SelectedProject.Id,
                Year = SelectedMonth.Year,
                Month = SelectedMonth.Month
            };

            var tcpMessage = new TCPMessageBase
            {
                Type = MessageType.ScheduleRequest,
                SenderIP = GlobalVariables.SelfInfo.IP,
                SenderName = GlobalVariables.SelfInfo.Name,
                MessageBody = new List<TCPMessageBody> { new TCPMessageBody { Content = JsonConvert.SerializeObject(request) } }
            };

            GlobalVariables.TcpServerClient.SendMessage(JsonConvert.SerializeObject(tcpMessage), SelectedUser.IP, GlobalVariables.TcpPort);
        }


        private void OnProjectListReceived(object sender, List<Project> receivedProjects)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (var project in receivedProjects)
                {
                    if (!Projects.Any(p => p.Id == project.Id))
                    {
                        Projects.Add(project);
                    }
                }
            });
        }

        private DateTime CalculateEndDate(DateTime startDate, int durationInWorkDays)
        {
            int workDaysCounted = 0;
            DateTime currentDate = startDate;
            var holidaysInYear = HolidayRepository.SelectHolidays(startDate.Year);

            while (workDaysCounted < durationInWorkDays)
            {
                if (currentDate.Year != holidaysInYear.FirstOrDefault()?.Year)
                {
                    holidaysInYear = HolidayRepository.SelectHolidays(currentDate.Year);
                }

                var holiday = holidaysInYear.FirstOrDefault(h => h.Year == currentDate.Year && h.Month == currentDate.Month && h.Day == currentDate.Day);
                bool isHoliday = holiday != null && holiday.IsHoliday;
                bool isWeekendWorkday = holiday != null && !holiday.IsHoliday && (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday);

                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (!isHoliday)
                    {
                        workDaysCounted++;
                    }
                }
                else if (isWeekendWorkday)
                {
                    workDaysCounted++;
                }

                if (workDaysCounted < durationInWorkDays)
                {
                    currentDate = currentDate.AddDays(1);
                }
            }
            return currentDate;
        }

        private void UpdateTask(Task task)
        {
            TaskRepository.RemoveTask(task.Id);

            DateTime startDate = new DateTime(task.Year, task.Month, task.StartDay);
            DateTime endDate = CalculateEndDate(startDate, task.Days);

            DateTime currentMonthStart = new DateTime(startDate.Year, startDate.Month, 1);
            while (currentMonthStart <= endDate)
            {
                DateTime currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);

                DateTime partStartDate = startDate > currentMonthStart ? startDate : currentMonthStart;
                DateTime partEndDate = endDate < currentMonthEnd ? endDate : currentMonthEnd;

                if (partStartDate > partEndDate)
                {
                    currentMonthStart = currentMonthStart.AddMonths(1);
                    continue;
                }

                var taskPart = new Task
                {
                    Id = task.Id,
                    ProjectId = task.ProjectId,
                    Name = task.Name,
                    Year = partStartDate.Year,
                    Month = partStartDate.Month,
                    StartDay = partStartDate.Day,
                    Days = (int)(partEndDate - partStartDate).TotalDays + 1,
                    Note = task.Note,
                };
                TaskRepository.InsertOrUpdateTask(taskPart);

                currentMonthStart = currentMonthStart.AddMonths(1);
            }
        }

        private void InitializeCommands()
        {
            AddTaskCommand = new RelayCommand(AddTask);
            EditTaskCommand = new RelayCommand<ScheduleTaskViewModel>(EditTask);
            DeleteTaskCommand = new RelayCommand<ScheduleTaskViewModel>(DeleteTask);
            SetAsHolidayCommand = new RelayCommand<object>(day => SetHoliday(day?.ToString(), true));
            SetAsWorkdayCommand = new RelayCommand<object>(day => SetHoliday(day?.ToString(), false));
        }

        private void AddTask()
        {
            if (SelectedProject == null) return;

            var newTask = new Task
            {
                Id = Guid.NewGuid().ToString(),
                ProjectId = SelectedProject.Id,
                Year = SelectedMonth.Year,
                Month = SelectedMonth.Month,
                StartDay = 1,
                Days = 1
            };

            var viewModel = new ViewModel.ModalDialog.EditTaskWindowViewModel(newTask);
            var window = new EditTaskWindow(viewModel);

            if (window.ShowDialog() == true)
            {
                viewModel.UpdateTask();
                UpdateTask(viewModel.Task);
                GenerateSchedule();
            }
        }

        private void EditTask(ScheduleTaskViewModel taskVm)
        {
            if (taskVm == null) return;

            var fullTaskParts = TaskRepository.SelectTasksById(taskVm.Task.Id);
            var firstPart = fullTaskParts.OrderBy(t => t.Year).ThenBy(t => t.Month).First();
            
            var totalWorkDays = GetWorkDays(firstPart, fullTaskParts).Count;

            var taskToEdit = new Task
            {
                Id = firstPart.Id,
                ProjectId = firstPart.ProjectId,
                Name = firstPart.Name,
                Year = firstPart.Year,
                Month = firstPart.Month,
                StartDay = firstPart.StartDay,
                Days = totalWorkDays,
                Note = firstPart.Note,
            };


            var viewModel = new ViewModel.ModalDialog.EditTaskWindowViewModel(taskToEdit);
            var window = new EditTaskWindow(viewModel);

            if (window.ShowDialog() == true)
            {
                viewModel.UpdateTask();
                UpdateTask(viewModel.Task);
                GenerateSchedule();
            }
        }

        private void DeleteTask(ScheduleTaskViewModel task)
        {
            if (task != null)
            {
                TaskRepository.RemoveTask(task.Task.Id);
                ProgressRepository.RemoveProgress(task.Task.Id);
                GenerateSchedule();
            }
        }

        private void SetHoliday(string dayString, bool isHoliday)
        {
            if (int.TryParse(dayString, out int day))
            {
                HolidayRepository.InsertOrUpdateHoliday(new Holiday
                {
                    Year = SelectedMonth.Year,
                    Month = SelectedMonth.Month,
                    Day = day,
                    IsHoliday = isHoliday
                });
                GenerateSchedule();
            }
        }

        private void LoadProjects()
        {
            Projects = new ObservableCollection<Project>(ProjectRepository.SelectProjects());
        }

        private void AddDummyData()
        {
        }

        public void GenerateSchedule()
        {
            if (SelectedProject == null)
            {
                ScheduleTasks = new ObservableCollection<ScheduleTaskViewModel>();
                return;
            }

            GenerateColumns?.Invoke(this, EventArgs.Empty);
            
            var allHolidaysForYear = HolidayRepository.SelectHolidays(SelectedMonth.Year);
            if (SelectedMonth.Year != DateTime.Now.Year)
            {
                allHolidaysForYear.AddRange(HolidayRepository.SelectHolidays(DateTime.Now.Year));
            }
            _holidays = allHolidaysForYear.Where(h => h.Month == SelectedMonth.Month).ToList();

            var tasksFromDb = TaskRepository.SelectTasksByMonth(SelectedProject?.Id, SelectedMonth.Year, SelectedMonth.Month);

            var scheduleTasks = new ObservableCollection<ScheduleTaskViewModel>();

            var taskGroups = tasksFromDb.GroupBy(t => t.Id);

            foreach (var group in taskGroups)
            {
                var firstPart = group.OrderBy(t => t.Year).ThenBy(t => t.Month).ThenBy(t => t.StartDay).First();
                var currentMonthPart = group.First(t => t.Year == SelectedMonth.Year && t.Month == SelectedMonth.Month);
                
                var progress = ProgressRepository.SelectProgress(firstPart.Id, SelectedProject.Id) ?? new Progress { TaskId = firstPart.Id, ProjectId = SelectedProject.Id, Percentage = 0 };
                var taskVm = new ScheduleTaskViewModel
                {
                    Name = firstPart.Name,
                    Task = currentMonthPart,
                    Progress = progress
                };
                taskVm.Percentage = progress.Percentage;
                taskVm.PercentageChanged += (s, e) =>
                {
                    progress.Percentage = (s as ScheduleTaskViewModel).Percentage;
                    ProgressRepository.InsertOrUpdateProgress(progress);
                    UpdateCellColors();
                };
                for (int i = 0; i < DaysInMonth; i++)
                {
                    taskVm.Days.Add(new DayCellViewModel());
                }
                scheduleTasks.Add(taskVm);
            }
            ScheduleTasks = new ObservableCollection<ScheduleTaskViewModel>(scheduleTasks.OrderBy(t=>t.Task.StartDay));

            UpdateCellColors();
        }

        private void UpdateCellColors()
        {
            if (ScheduleTasks == null) return;

            DateTime today = DateTime.Today;
            DateTime firstDayOfMonth = new DateTime(SelectedMonth.Year, SelectedMonth.Month, 1);
            var holidaysInMonth = HolidayRepository.SelectHolidays(SelectedMonth.Year, SelectedMonth.Month)
                                    .ToDictionary(h => h.Day, h => h.IsHoliday);

            foreach (var taskVm in ScheduleTasks)
            {
                var allParts = TaskRepository.SelectTasksById(taskVm.Task.Id);
                var workDaysInTask = GetWorkDays(taskVm.Task, allParts);
                double completedWorkDays = workDaysInTask.Count * (taskVm.Percentage / 100.0);
                int completedDaysCounter = 0;

                var expectedWorkDaysToToday = workDaysInTask.Count(d => d <= today);
                bool isDelayed = completedWorkDays < expectedWorkDaysToToday;

                DateTime taskPartStartDate = new DateTime(taskVm.Task.Year, taskVm.Task.Month, taskVm.Task.StartDay);
                DateTime taskPartEndDate = taskPartStartDate.AddDays(taskVm.Task.Days - 1);

                for (int i = 0; i < DaysInMonth; i++)
                {
                    DateTime currentDate = firstDayOfMonth.AddDays(i);
                    var dayCell = taskVm.Days[i];

                    dayCell.IsToday = (currentDate == today);
                    
                    bool isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;
                    bool isHoliday = holidaysInMonth.TryGetValue(currentDate.Day, out bool isHolidayValue) && isHolidayValue;
                    bool isWorkdayOverride = holidaysInMonth.TryGetValue(currentDate.Day, out bool isWorkdayOverrideValue) && !isWorkdayOverrideValue;

                    dayCell.IsHolidayOrWeekend = (isWeekend && !isWorkdayOverride) || isHoliday;
                    
                    bool isWorkDay = !dayCell.IsHolidayOrWeekend;
                    
                    dayCell.IsTaskDay = (currentDate >= taskPartStartDate && currentDate <= taskPartEndDate);

                    if (dayCell.IsTaskDay && isWorkDay)
                    {
                        completedDaysCounter++;
                        dayCell.IsCompleted = (completedDaysCounter <= completedWorkDays);
                        dayCell.IsDelayed = (!dayCell.IsCompleted && isDelayed);
                    }
                    else
                    {
                        dayCell.IsCompleted = false;
                        dayCell.IsDelayed = false;
                    }
                }
            }
        }

        private List<DateTime> GetWorkDays(Task task, List<Task> allParts)
        {
            var workDays = new List<DateTime>();
            var firstPart = allParts.OrderBy(p => p.Year).ThenBy(p => p.Month).First();
            var lastPart = allParts.OrderBy(p => p.Year).ThenBy(p => p.Month).Last();
            DateTime startDate = new DateTime(firstPart.Year, firstPart.Month, firstPart.StartDay);
            DateTime endDate = new DateTime(lastPart.Year, lastPart.Month, lastPart.StartDay).AddDays(lastPart.Days - 1);

            var holidays = HolidayRepository.SelectHolidaysForRange(startDate, endDate);

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var holiday = holidays.FirstOrDefault(h => h.Year == date.Year && h.Month == date.Month && h.Day == date.Day);
                bool isHoliday = holiday != null && holiday.IsHoliday;
                bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                bool isWeekendWorkday = holiday != null && !holiday.IsHoliday && isWeekend;

                if ((!isWeekend && !isHoliday) || isWeekendWorkday)
                {
                    workDays.Add(date);
                }
            }
            return workDays;
        }

        private void AddProject()
        {
            var viewModel = new ViewModel.ModalDialog.AddProjectWindowViewModel();
            var window = new AddProjectWindow(viewModel);

            if (window.ShowDialog() == true)
            {
                var project = new Project
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = viewModel.Name,
                    Managers = $"{GlobalVariables.SelfInfo.IP};{GlobalVariables.SelfInfo.Name}"
                };
                ProjectRepository.InsertOrUpdateProject(project);
                LoadProjects();
                SelectedProject = Projects.FirstOrDefault(p => p.Id == project.Id);
            }
        }

        private void ManageProject()
        {
            if (SelectedProject == null) return;

            var viewModel = new ViewModel.ModalDialog.ManageProjectWindowViewModel(SelectedProject);
            var window = new ManageProjectWindow(viewModel);

            if (window.ShowDialog() == true)
            {
                SelectedProject.Managers = string.Join(";", viewModel.Managers.Select(u => $"{u.IP};{u.Name}"));
                ProjectRepository.InsertOrUpdateProject(SelectedProject);
                LoadProjects();
            }
        }

        private void Refresh()
        {
            var tcpMessage = new TCPMessageBase()
            {
                Type = MessageType.ProjectListRequest,
                SenderIP = GlobalVariables.SelfInfo.IP,
                SenderName = GlobalVariables.SelfInfo.Name
            };
            var message = JsonConvert.SerializeObject(tcpMessage);
            // UdpBroadcaster.Broadcast(message);
            GenerateSchedule();
        }
    }
}
