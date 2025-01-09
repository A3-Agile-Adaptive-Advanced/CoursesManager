using CoursesManager.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoursesManager.UI.Views.Controls.CoursesCalendar
{
    public partial class CalendarLayout : UserControl, INotifyPropertyChanged
    {
        private static readonly SolidColorBrush BlueColorBrush = new SolidColorBrush(Color.FromRgb(49, 43, 127));
        private static readonly SolidColorBrush WhiteColorBrush = new SolidColorBrush(Colors.White);

        private DateTime _selectedDate = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        private CalendarDay _selectedDay;

        public static readonly DependencyProperty CoursesProperty = DependencyProperty.Register(
            nameof(Courses),
            typeof(ObservableCollection<Course>),
            typeof(CalendarLayout),
            new PropertyMetadata(null, OnCoursesChanged)
        );

        public static readonly DependencyProperty OnDateChangedCommandProperty = DependencyProperty.Register(
            nameof(OnDateChangedCommand),
            typeof(ICommand),
            typeof(CalendarLayout),
            new PropertyMetadata(null)
        );

        public static readonly DependencyProperty OnDaySelectedCommandProperty = DependencyProperty.Register(
            nameof(OnDaySelectedCommand),
            typeof(ICommand),
            typeof(CalendarLayout),
            new PropertyMetadata(null)
        );

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<CalendarDay> CalendarDayClickedEvent;

        public CalendarLayout()
        {
            InitializeComponent();
        }

        /// <summary>
        /// A list of <see cref="Course"/> objects that could be displayed on the calendar.
        /// </summary>
        public ObservableCollection<Course> Courses
        {
            get => (ObservableCollection<Course>)GetValue(CoursesProperty);
            set => SetValue(CoursesProperty, value);
        }

        /// <summary>
        /// A command that fires whenever <see cref="SelectedDate"/> changes.
        /// </summary>
        public ICommand OnDateChangedCommand
        {
            get => (ICommand)GetValue(OnDateChangedCommandProperty);
            set => SetValue(OnDateChangedCommandProperty, value);
        }

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
        public ObservableCollection<CalendarDay> DaysInCurrentView { get; } = new();

        /// <summary>
        /// The currently selected date. Changing this triggers redraw logic.
        /// </summary>
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate == value)
                    return;

                _selectedDate = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(PreviousYearNumber));
                OnPropertyChanged(nameof(NextYearNumber));
                OnPropertyChanged(nameof(PreviousMonthNumber));
                OnPropertyChanged(nameof(NextMonthNumber));


                DrawDays();

                OnDateChangedCommand?.Execute(this);

                DrawItems();
            }
        }


        /// <summary>
        /// Read-only property for the year label (previous year).
        /// </summary>
        public string PreviousYearNumber => _selectedDate.AddYears(-1).Year.ToString();

        /// <summary>
        /// Read-only property for the year label (next year).
        /// </summary>
        public string NextYearNumber => _selectedDate.AddYears(1).Year.ToString();

        /// <summary>
        /// Read-only property for the month label (previous month).
        /// </summary>
        public string PreviousMonthNumber => _selectedDate.AddMonths(-1).Month.ToString();

        /// <summary>
        /// Read-only property for the month label (next month).
        /// </summary>
        public string NextMonthNumber => _selectedDate.AddMonths(1).Month.ToString();

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
            if (Courses == null)
                return;

            if (Courses is ObservableCollection<Course> courses)
            {
                foreach (var course in courses)
                {
                    int eventRow = 0;

                    DateTime startDate = course.StartDate;
                    DateTime endDate = course.EndDate;

                    CalendarDay dayByStartDate = DaysInCurrentView.Where(day => day.Date == course.StartDate).FirstOrDefault();
                    if (dayByStartDate != null)
                    {
                        CalendarItem newItem = new();
                        int currentAmountOfItemsOnDay = dayByStartDate.Items.Children.Count;

                        if (currentAmountOfItemsOnDay > 0)
                            eventRow = Grid.GetRow(dayByStartDate.Items.Children[currentAmountOfItemsOnDay - 1]) + 1;
                        Grid.SetRow(newItem, eventRow);

                        if (currentAmountOfItemsOnDay < 4)
                        {
                            if (currentAmountOfItemsOnDay < 3)
                            {
                                newItem.Label = course.Code;
                                newItem.IsStartItem = true;
                            }
                            else
                                newItem.Label = "...";

                            dayByStartDate.Items.Children.Add(newItem);
                        }
                    }

                    CalendarDay dayByEndDate = DaysInCurrentView.Where(day => day.Date == course.EndDate).FirstOrDefault();
                    if (dayByEndDate != null)
                    {
                        CalendarItem newItem = new();
                        int currentAmountOfItemsOnDay = dayByEndDate.Items.Children.Count;
                        
                        if (currentAmountOfItemsOnDay > 0)
                            eventRow = Grid.GetRow(dayByEndDate.Items.Children[currentAmountOfItemsOnDay - 1]) + 1;
                        Grid.SetRow(newItem, eventRow);

                        if (currentAmountOfItemsOnDay < 4)
                        {
                            if (currentAmountOfItemsOnDay < 3)
                            {
                                newItem.Label = course.Code;
                            }
                            else
                                newItem.Label = "...";

                            dayByEndDate.Items.Children.Add(newItem);
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException("Events must be IEnumerable<ICalendarEvent>");
            }
        }

        /// <summary>
        /// Invokes PropertyChanged event.
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (_selectedDay != null && date.Date == _selectedDay.Date)
                {
                    _selectedDay.Items.Children.Clear();
                    DaysInCurrentView.Add(_selectedDay);

                    continue;
                }

                CalendarDay _newDay = new()
                {
                    Date = date,
                    IsSelectedMonth = (date.Month == _selectedDate.Month),
                };

                _newDay.MouseLeftButtonDown += OnDaySelected;
                DaysInCurrentView.Add(_newDay);
            }
        }

        private void PlaceDaysInGrid()
        {
            int row = 0;
            foreach (CalendarDay day in DaysInCurrentView)
            {
                // Sunday => column 6, Monday => column 0
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

                day.BorderBrush = new SolidColorBrush(Color.FromRgb(49, 43, 127));

                DaysGrid.Children.Add(day);

                // Advance to the nect row if we reach Sunday.
                if (column == 6) row++;
            }
        }

        private static void OnCoursesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ObservableCollection<Course> newCollection)
            {
                var viewModel = d as CalendarLayout;
                viewModel?.OnCoursesUpdated(newCollection);
            }
        }

        private void OnCoursesUpdated(ObservableCollection<Course> courses) => DrawItems();

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
            if (sender is not CalendarDay selectedDay)
                return;

            foreach (CalendarDay day in DaysGrid.Children.OfType<CalendarDay>())
            {
                day.Border.Background = (day == selectedDay)
                    ? BlueColorBrush
                    : WhiteColorBrush;
                day.DateTextBlock.Foreground = (day == selectedDay)
                    ? WhiteColorBrush
                    : BlueColorBrush;
            }

            _selectedDay = selectedDay;
            OnDaySelectedCommand?.Execute(selectedDay);
        }
    }
}
