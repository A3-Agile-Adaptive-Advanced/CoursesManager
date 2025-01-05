namespace CoursesManager.UI.Service
{
    public interface IFileDialogService
    {
        bool ShowDialog();
        string FileName { get; } 
    }
}
