using System;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.LocationRepository;
using CoursesManager.UI.ViewModels.Courses;
using Microsoft.Win32;


public class TestableCourseDialogViewModel : CourseDialogViewModel
{
    private readonly OpenFileDialog _mockDialog;

    public TestableCourseDialogViewModel(
        ICourseRepository courseRepository,
        IDialogService dialogService,
        ILocationRepository locationRepository,
        Course? course,
        OpenFileDialog mockDialog)
        : base(courseRepository, dialogService, locationRepository, course)
    {
        _mockDialog = mockDialog;
    }

    protected override OpenFileDialog CreateOpenFileDialog()
    {
        return _mockDialog;
    }
}