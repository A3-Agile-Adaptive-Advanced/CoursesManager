﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Enums;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;

namespace CoursesManager.UI.ViewModels.Students
{
    /// <summary>
    /// ViewModel responsible for managing the details of a specific student.
    /// Includes loading course details, navigating to edit dialogs, and updating UI-bound properties.
    /// required to manage the "StudentDetailView" dialog.
    /// </summary>
    public class StudentDetailViewModel : ViewModelWithNavigation
    {
        private readonly IRegistrationRepository _registrationRepository;
        private readonly INavigationService _navigation;
        private readonly ICourseRepository _courseRepository;
        private readonly IDialogService _dialogService;
        private readonly IMessageBroker _messageBroker;

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand EditStudent { get; }
        private Student _student;
        private ObservableCollection<CourseStudentPayment> _courseDetails = new ObservableCollection<CourseStudentPayment>();

        public ObservableCollection<CourseStudentPayment> CourseDetails
        {
            get => _courseDetails;
            private set
            {
                _courseDetails = value;
                OnPropertyChanged(nameof(CourseDetails));
            }
        }
        public StudentDetailViewModel(
            IDialogService dialogService,
            IMessageBroker messageBroker,
            IRegistrationRepository registrationRepository,
            INavigationService navigationService,
            Student? student) : base(navigationService)
        {
            _dialogService = dialogService;
            _messageBroker = messageBroker;
            _registrationRepository = registrationRepository;
            Student = student ?? new Student();
            EditStudent = new RelayCommand<Student>(OpenEditStudentPopup, s => s != null);
            LoadStudentDetails();
        }

        public Student Student
        {
            get => _student;
            set
            {
                if (_student != value)
                {
                    _student = value;
                    OnPropertyChanged(nameof(Student));
                }
            }
        }
        private void LoadStudentDetails()
        {
            if (Student == null)
            {
                return;
            }

            var registrations = _registrationRepository.GetAll().Where(r => r.StudentId == Student.Id);

            CourseDetails.Clear();

            foreach (var registration in registrations)
            {
                CourseDetails.Add(new CourseStudentPayment(registration.Course, registration));
            }

            OnPropertyChanged(nameof(CourseDetails));
        }

        private async void OpenEditStudentPopup(Student student)
        {
            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<EditStudentViewModel, Student>(student);

                if (dialogResult?.Outcome == DialogOutcome.Success)
                {
                    _messageBroker.Publish(new ToastNotificationMessage(true,
                        "Student succesvol gewijzigd.", ToastType.Confirmation));
                    LoadStudentDetails();
                }
            });
        }

        /// Triggers the PropertyChanged event for a given property name.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}