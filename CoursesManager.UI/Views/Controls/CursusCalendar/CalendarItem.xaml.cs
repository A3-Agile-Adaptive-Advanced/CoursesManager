using System.Windows.Controls;

namespace CoursesManager.UI.Views.Controls.CursusCalendar
{
    public partial class CalendarItem : UserControl
    {
        public string Label { get; set; }
        public bool IsStartItem { get; set; }

        public CalendarItem()
        {
            InitializeComponent();
        }
    }
}
