using System.Windows.Controls;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CoursesManager.UI.Views.Controls.CursusCalendar
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
