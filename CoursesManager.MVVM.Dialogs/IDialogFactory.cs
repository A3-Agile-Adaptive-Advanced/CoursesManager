using System.Windows;

namespace CoursesManager.MVVM.Dialogs;

public interface IDialogFactory
{
    Window SetupWindow(Type windowType);

    void OpenDialog(Window window);
}