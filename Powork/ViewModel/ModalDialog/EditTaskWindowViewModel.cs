using CommunityToolkit.Mvvm.ComponentModel;
using Powork.Model;
using System;
using Task = Powork.Model.Task;

namespace Powork.ViewModel.ModalDialog
{
    public partial class EditTaskWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private int _days;

        [ObservableProperty]
        private string _note;

        public Task Task { get; }

        public EditTaskWindowViewModel(Task task)
        {
            Task = task;
            Name = task.Name;
            StartDate = new DateTime(task.Year, task.Month, task.StartDay);
            Days = task.Days;
            Note = task.Note;
        }

        public void UpdateTask()
        {
            Task.Name = Name;
            Task.Year = StartDate.Year;
            Task.Month = StartDate.Month;
            Task.StartDay = StartDate.Day;
            Task.Days = Days;
            Task.Note = Note;
        }
    }
}
