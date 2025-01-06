using System.Windows.Controls;

namespace CoursesManager.UI.Views.Controls.CoursesCalendar
{
    public partial class CalendarDay : UserControl
    {
        public DateTime Date { get; set; }
        public bool IsSelectedMonth { get; set; }

        public bool IsToday => (Date == DateTime.Today);

        public CalendarDay()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
