using CommunityToolkit.Mvvm.ComponentModel;
using Powork.Model;
using Powork.Repository;
using System.Collections.ObjectModel;
using Task = Powork.Model.Task;

namespace Powork.ViewModel.Inner
{
    public partial class ScheduleTaskViewModel : ObservableObject
    {
        public Task Task { get; set; }
        public Progress Progress { get; set; }

        [ObservableProperty]
        private string _name;

        private int _percentage;
        public int Percentage
        {
            get => _percentage;
            set
            {
                if (SetProperty(ref _percentage, value))
                {
                    Progress.Percentage = value;
                    ProgressRepository.InsertOrUpdateProgress(Progress);
                    PercentageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler PercentageChanged;

        [ObservableProperty]
        private ObservableCollection<DayCellViewModel> _days;

        public ScheduleTaskViewModel()
        {
            Days = new ObservableCollection<DayCellViewModel>();
        }
    }
}
