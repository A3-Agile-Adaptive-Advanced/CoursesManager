using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Utils;
using CoursesManager.UI.Models;
using CoursesManager.UI.Models.CoursesManager.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CoursesManager.MVVM.Messages;

namespace CoursesManager.UI.ViewModels.Courses
{
    class CourseOverViewViewModel : ViewModel
    {
        public IMessageBroker _messageBroker {  get; set; }
        public ICommand ChangeCourseCommand { get; set; }
        public ICommand DeleteCourseCommand { get; set; }
        public Course CurrentCourse { get; set; }
        public ObservableCollection<Student>? Students { get; set; }
        public ObservableCollection<CourseStudentPayment>? studentPayments { get; set; }


        public CourseOverViewViewModel(IMessageBroker MessageBroker)
        {

            _messageBroker = MessageBroker;
            ChangeCourseCommand = new RelayCommand(ChangeCourse);
            DeleteCourseCommand = new RelayCommand(DeleteCourse);
            CurrentCourse = (Course)GlobalCache.Instance.Get("Opened Course");
            ViewTitle = CurrentCourse.Name;
            Students = CurrentCourse.students;
            ObservableCollection<Registration> registration = DummyDataGenerator.GenerateRegistrations(Students.Count, 1);
            studentPayments = new ObservableCollection<CourseStudentPayment>();

                for (int i = 0 ; i < registration.Count - 1 ; i++)  {
                CourseStudentPayment studentPayment = new CourseStudentPayment(Students[i], registration[i]);
                studentPayments.Add(studentPayment);
                }
        }


        private void ChangeCourse()
        {
            
        }

        private void DeleteCourse()
        {

        }
    }
}
