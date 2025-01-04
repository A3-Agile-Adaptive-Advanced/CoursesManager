using CoursesManager.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoursesManager.UI.Views.Controls.CursusCalendar
{
    public partial class CalendarLayout : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event EventHandler<CalendarDay> CalendarDayClickedEvent;

        private DateTime _selectedDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged();

                    // Update the four read-only properties below
                    OnPropertyChanged(nameof(PreviousYearNumber));
                    OnPropertyChanged(nameof(NextYearNumber));
                    OnPropertyChanged(nameof(PreviousMonthNumber));
                    OnPropertyChanged(nameof(NextMonthNumber));

                    OnDateChangedCommand?.Execute(_selectedDate);

                    DrawDays();
                }
            }
        }

        public  string PreviousYearNumber => _selectedDate.AddYears(-1).Year.ToString();
        public string NextYearNumber => _selectedDate.AddYears(1).Year.ToString();
        public string PreviousMonthNumber => _selectedDate.AddMonths(-1).Month.ToString();
        public string NextMonthNumber => _selectedDate.AddMonths(1).Month.ToString();

        public static readonly DependencyProperty CoursesProperty = DependencyProperty.Register(
           nameof(Courses),
           typeof(List<Course>),
           typeof(CalendarLayout),
           new FrameworkPropertyMetadata(null)
        );

        /// <summary>
        /// A list of <see cref="Course"/> objects that could be displayed on the calendar.
        /// </summary>
        public List<Course> Courses
        {
            get { return (List<Course>)GetValue(CoursesProperty); }
            set { SetValue(CoursesProperty, value); }
        }

        public static readonly DependencyProperty OnDateChangedCommandProperty = DependencyProperty.Register(
            nameof(OnDateChangedCommand),
            typeof(ICommand),
            typeof(CalendarLayout),
            new PropertyMetadata(null)
        );

        /// <summary>
        /// A command that fires whenever <see cref="SelectedDate"/> changes.
        /// </summary>
        public ICommand OnDateChangedCommand
        {
            get => (ICommand)GetValue(OnDateChangedCommandProperty);
            set => SetValue(OnDateChangedCommandProperty, value);
        }

        public static readonly DependencyProperty OnDaySelectedCommandProperty = DependencyProperty.Register(
           nameof(OnDaySelectedCommand),
           typeof(ICommand),
           typeof(CalendarLayout),
           new PropertyMetadata(null)
       );

        /// <summary>
        /// A command that fires whenever <see cref="SelectedDate"/> changes.
        /// </summary>
        public ICommand OnDaySelectedCommand
        {
            get => (ICommand)GetValue(OnDaySelectedCommandProperty);
            set => SetValue(OnDaySelectedCommandProperty, value);
        }

        /// <summary>
        /// The collection of days (including previous / next month filler days)
        /// that are currently displayed on the calendar.
        /// </summary>
        public ObservableCollection<CalendarDay> DaysInCurrentView { get; } = [];

        public CalendarLayout()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void DrawDays()
        {
            DaysGrid.Children.Clear();
            DaysInCurrentView.Clear();

            (DateTime monthStart, DateTime monthEnd) = GetMonthBoundaries(_selectedDate);

            int amountToPrepend = GetDaysToPrepend(monthStart);
            if (amountToPrepend > 0)
            {
                DateTime prevStart = monthStart.AddDays(-amountToPrepend);
                DateTime prevEnd = monthStart.AddDays(-1);
                AddDaysToCollection(prevStart, prevEnd);
            }

            AddDaysToCollection(monthStart, monthEnd);

            int amountToAppend = GetDaysToAppend(monthEnd);
            if (amountToAppend > 0)
            {
                DateTime nextStart = monthEnd.AddDays(1);
                DateTime nextEnd = monthEnd.AddDays(amountToAppend);
                AddDaysToCollection(nextStart, nextEnd);
            }

            PlaceDaysInGrid();
        }

        public void DrawItems()
        {
            // Future expansion
        }

        /// <summary>
        /// Returns the first and last day of the month for the given date.
        /// </summary>
        private (DateTime firstDay, DateTime lastDay) GetMonthBoundaries(DateTime date)
        {
            DateTime firstDay = new(date.Year, date.Month, 1);
            DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);
            return (firstDay, lastDay);
        }

        private int GetDaysToPrepend(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Monday) return 0;

            // Sunday has an enum value of 0, so treat it as 7 for calculations
            int offset = (int)date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek;

            // Align Monday as column 0
            return offset - 1;
        }

        private int GetDaysToAppend(DateTime date)
        {
            // If last day isn't Sunday, fill up the rest of the grid week
            return date.DayOfWeek == DayOfWeek.Sunday
                ? 0
                : 7 - (int)date.DayOfWeek;
        }

        private void AddDaysToCollection(DateTime startDate, DateTime endDate)
        {
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1)) {
                var _newDay = new CalendarDay
                {
                    Date = date,
                    IsSelectedMonth = (date.Month == _selectedDate.Month),
                    IsToday = (date.Date == DateTime.Today)
                };

                _newDay.MouseLeftButtonDown += OnDaySelected;

                DaysInCurrentView.Add(_newDay);
            }
        }

        private void PlaceDaysInGrid()
        {
            int row = 0;
            foreach (var day in DaysInCurrentView)
            {
                int column = (day.Date.DayOfWeek == DayOfWeek.Sunday)
                    ? 6
                    : ((int)day.Date.DayOfWeek - 1);

                Grid.SetRow(day, row);
                Grid.SetColumn(day, column);

                day.BorderThickness = new Thickness(
                    left: column != 0 ? 0 : 1,
                    top: row != 0 ? 0 : 1,
                    right: 1,
                    bottom: 1
                );
                day.BorderBrush = new SolidColorBrush(Color.FromRgb(49,43,127));

                DaysGrid.Children.Add(day);

                // Advance to the nect row if we reach Sunday.
                if (column == 6) row++;
            }
        }

        private void PreviousYearButton_OnClick(object sender, RoutedEventArgs e)
            => SelectedDate = SelectedDate.AddYears(-1);
        private void NextYearButton_OnClick(object sender, RoutedEventArgs e)
            => SelectedDate = SelectedDate.AddYears(1);

        private void PreviousMonthButton_OnClick(object sender, RoutedEventArgs e)
            => SelectedDate = SelectedDate.AddMonths(-1);

        private void NextMonthButton_OnClick(object sender, RoutedEventArgs e)
            => SelectedDate = SelectedDate.AddMonths(1);

        private void OnDaySelected(object sender, MouseButtonEventArgs e)
        {
            if (sender is CalendarDay clickedDay)
            {
                if (OnDaySelectedCommand?.CanExecute(clickedDay) == true)
                {
                    OnDaySelectedCommand.Execute(clickedDay);
                }
            }
        }
    }
}
