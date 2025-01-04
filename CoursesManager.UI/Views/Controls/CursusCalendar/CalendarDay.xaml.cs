using System.Windows.Controls;
using System.Windows.Input;

namespace CoursesManager.UI.Views.Controls.CursusCalendar
{
    public partial class CalendarDay : UserControl
    {
        public DateTime Date { get; set; }
        public bool IsSelectedMonth { get; set; }
        public bool IsToday { get; set; }

        public CalendarDay()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
