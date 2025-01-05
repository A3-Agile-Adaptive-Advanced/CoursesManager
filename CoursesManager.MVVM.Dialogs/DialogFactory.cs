using System.Windows;

namespace CoursesManager.MVVM.Dialogs;

public class DialogFactory : IDialogFactory
{
    public Window SetupWindow(Type windowType)
    {
        return (Window)Activator.CreateInstance(windowType)!;
    }

    public void OpenDialog(Window window)
    {
        window.ShowDialog();
    }
}