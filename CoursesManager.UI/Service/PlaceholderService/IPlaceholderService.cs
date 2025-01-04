using CoursesManager.UI.Models;

namespace CoursesManager.UI.Service.PlaceholderService
{
    public interface IPlaceholderService
    {
        List<string> GetValidPlaceholders();
        List<string> ValidatePlaceholders(string htmlString);
    }
}
