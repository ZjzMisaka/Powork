using CommunityToolkit.Mvvm.ComponentModel;

namespace Powork.ViewModel.Inner
{
    public partial class DayCellViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isToday;

        [ObservableProperty]
        private bool _isHolidayOrWeekend;

        [ObservableProperty]
        private bool _isTaskDay;

        [ObservableProperty]
        private bool _isCompleted;
        
        [ObservableProperty]
        private bool _isDelayed;
    }
}
