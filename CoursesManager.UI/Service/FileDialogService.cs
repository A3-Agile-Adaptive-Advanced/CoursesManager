using CoursesManager.UI.Service;
using Microsoft.Win32;

public class FileDialogService : IFileDialogService
{
    private readonly OpenFileDialog _openFileDialog = new OpenFileDialog
    {
        Filter = "Image Files|*.bmp;*.jpg;*.png",
        FilterIndex = 1
    };

    public bool ShowDialog() => _openFileDialog.ShowDialog() == true;

    public string FileName => _openFileDialog.FileName;
}
