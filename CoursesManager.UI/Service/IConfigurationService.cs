
using CoursesManager.UI.Models;

namespace CoursesManager.UI.Service
{
    public interface IConfigurationService
    {
        bool ValidateSettings();
        Dictionary<string, string> GetDatabaseParameters();
        Dictionary<string, string> GetMailParameters();
        void SaveEnvSettings(Dictionary<string, string> dbParams, Dictionary<string, string> mailParams);
    }
}