using Powork.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Powork.View
{
    /// <summary>
    /// SchedulePage.xaml 的交互逻辑
    /// </summary>
    public partial class SchedulePage : Page
    {
        private SchedulePageViewModel _viewModel;

        public SchedulePage()
        {
            InitializeComponent();
            _viewModel = new SchedulePageViewModel();
            DataContext = _viewModel;
            this.Loaded += SchedulePage_Loaded;
        }

        private void SchedulePage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.GenerateColumns += OnGenerateColumns;
            _viewModel.GenerateSchedule();
        }

        private void OnGenerateColumns(object sender, System.EventArgs e)
        {
            ScheduleGrid.Columns.Clear();
            ScheduleGrid.Columns.Add(new DataGridTextColumn() { Header = "Task", Binding = new Binding("Name"), IsReadOnly = true, Width = 200 });
            var percentageColumn = new DataGridTemplateColumn() { Header = "%", Width = 50 };
            string percentageTemplate =
                    @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                        <TextBox Text=""{Binding Percentage, UpdateSourceTrigger=PropertyChanged}"" />
                      </DataTemplate>";
            percentageColumn.CellTemplate = (DataTemplate)XamlReader.Parse(percentageTemplate);
            ScheduleGrid.Columns.Add(percentageColumn);

            for (int i = 0; i < _viewModel.DaysInMonth; i++)
            {
                var column = new DataGridTemplateColumn
                {
                    Header = (i + 1).ToString(),
                    Width = 30,
                    HeaderStyle = (Style)FindResource("DateHeaderStyle")
                };

                string templateString =
                    @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                        <Border>
                            <Border.Style>
                                <Style TargetType=""Border"">
                                    <Setter Property=""Background"" Value=""Transparent""/>
                                    <Setter Property=""BorderBrush"" Value=""LightGray""/>
                                    <Setter Property=""BorderThickness"" Value=""0,0,1,1""/>
                                    <Style.Triggers>
                                        <DataTrigger Binding=""{Binding Days[" + i + @"].IsHolidayOrWeekend}"" Value=""True"">
                                            <Setter Property=""Background"" Value=""LightGray""/>
                                        </DataTrigger>
                                        <DataTrigger Binding=""{Binding Days[" + i + @"].IsTaskDay}"" Value=""True"">
                                            <Setter Property=""Background"" Value=""CornflowerBlue""/>
                                        </DataTrigger>
                                        <DataTrigger Binding=""{Binding Days[" + i + @"].IsCompleted}"" Value=""True"">
                                            <Setter Property=""Background"" Value=""RoyalBlue""/>
                                        </DataTrigger>
                                        <DataTrigger Binding=""{Binding Days[" + i + @"].IsDelayed}"" Value=""True"">
                                            <Setter Property=""Background"" Value=""IndianRed""/>
                                        </DataTrigger>
                                        <DataTrigger Binding=""{Binding Days[" + i + @"].IsToday}"" Value=""True"">
                                            <Setter Property=""BorderBrush"" Value=""Red""/>
                                            <Setter Property=""BorderThickness"" Value=""2""/>
                                        </DataTrigger>
                                    </Style.Triggers>
                                </Style>
                            </Border.Style>
                        </Border>
                      </DataTemplate>";

                column.CellTemplate = (DataTemplate)XamlReader.Parse(templateString);
                ScheduleGrid.Columns.Add(column);
            }
        }
    }
}