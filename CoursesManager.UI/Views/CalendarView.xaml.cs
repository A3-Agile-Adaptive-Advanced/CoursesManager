using System.Windows.Controls;

namespace CoursesManager.UI.Views
{
    /// <summary>
    /// Interaction logic for CalendarView.xaml
    /// </summary>
    public partial class CalendarView : UserControl
    {
        public CalendarView()
        {
            InitializeComponent();
            CustomCalendar.DrawDays();
        }
    }
}
