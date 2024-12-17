
using System.Windows;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Models;
using CoursesManager.UI.Service;

namespace CoursesManager.UI.ViewModels
{
    public class ConfigurationViewModel : ViewModel
    {
        private readonly IConfigurationService _configurationService;



        private string _dbServer;
        public string DbServer
        {
            get => _dbServer;
            set => SetProperty(ref _dbServer, value);
        }

        private string _dbPort;
        public string DbPort
        {
            get => _dbPort;
            set => SetProperty(ref _dbPort, value);
        }

        private string _dbUser;
        public string DbUser
        {
            get => _dbUser;
            set => SetProperty(ref _dbUser, value);
        }

        private string _dbPassword;
        public string DbPassword
        {
            get => _dbPassword;
            set => SetProperty(ref _dbPassword, value);
        }



        private string _dbName;
        public string DbName
        {
            get => _dbName;
            set => SetProperty(ref _dbName, value);
        }

        // Mail server properties
        private string _mailServer;
        public string MailServer
        {
            get => _mailServer;
            set => SetProperty(ref _mailServer, value);
        }

        private string _mailPort;
        public string MailPort
        {
            get => _mailPort;
            set => SetProperty(ref _mailPort, value);
        }

        private string _mailUser;
        public string MailUser
        {
            get => _mailUser;
            set => SetProperty(ref _mailUser, value);
        }

        private string _mailPassword;
        public string MailPassword
        {
            get => _mailPassword;
            set => SetProperty(ref _mailPassword, value);
        }

        

        private EnvModel _appConfig;
        public EnvModel AppConfig
        {
            get => _appConfig;
            set => SetProperty(ref _appConfig, value);
        }

        public ICommand SaveCommand { get; }

        public ConfigurationViewModel(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            
            InitializeSettings();
            SaveCommand = new RelayCommand(ValidateAndSave);

            if (!CanSave() || !_configurationService.ValidateSettings())
            {
                MessageBox.Show("Instellingen zijn ongeldig. Controleer de ingevoerde waarden.");
            }
        }

        public void InitializeSettings()
        {
            var dbParams = _configurationService.GetDatabaseParameters();
            DbServer = dbParams.TryGetValue("Server", out var server) ? server : string.Empty;
            DbPort = dbParams.TryGetValue("Port", out var port) ? port : string.Empty;
            DbUser = dbParams.TryGetValue("User", out var user) ? user : string.Empty;
            DbPassword = dbParams.TryGetValue("Password", out var password) ? password : string.Empty;
            DbName = dbParams.TryGetValue("Database", out var dbName) ? dbName : string.Empty;

            var mailParams = _configurationService.GetMailParameters();
            MailServer = mailParams.TryGetValue("Server", out var mailServer) ? mailServer : string.Empty;
            MailPort = mailParams.TryGetValue("Port", out var mailPort) ? mailPort : string.Empty;
            MailUser = mailParams.TryGetValue("User", out var mailUser) ? mailUser : string.Empty;
            MailPassword = mailParams.TryGetValue("Password", out var mailPassword) ? mailPassword : string.Empty;

            Console.WriteLine($"DbServer: {DbServer}, DbPort: {DbPort}, DbUser: {DbUser}, DbPassword: {DbPassword}, DbName: {DbName}");
            Console.WriteLine($"MailServer: {MailServer}, MailPort: {MailPort}, MailUser: {MailUser}, MailPassword: {MailPassword}");
        }

        public void ValidateAndSave()
        {
            try
            {
                if (!CanSave())
                {
                    MessageBox.Show("Instellingen zijn ongeldig. Controleer de ingevoerde waarden.");
                }

                var dbParams = new Dictionary<string, string>
                {
                    { "Server", DbServer },
                    { "Port", DbPort },
                    { "User", DbUser },
                    { "Password", DbPassword },
                    { "Database", DbName }
                };

                var mailParams = new Dictionary<string, string>
                {
                    { "Server", MailServer },
                    { "Port", MailPort },
                    { "User", MailUser },
                    { "Password", MailPassword }
                };

                // Save encrypted data
                _configurationService.SaveEnvSettings(dbParams, mailParams);

                if (!_configurationService.ValidateSettings())
                {
                    MessageBox.Show("Instellingen zijn ongeldig. Controleer de ingevoerde waarden.");
                    INavigationService.CanNavigate = false;
                    return;
                }

                INavigationService.CanNavigate = true;


                MessageBox.Show("Instellingen succesvol opgeslagen!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij opslaan: {ex.Message}");
            }
        }


        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(DbServer) &&
                   !string.IsNullOrWhiteSpace(DbPort) &&
                   !string.IsNullOrWhiteSpace(DbUser) &&
                   !string.IsNullOrWhiteSpace(DbPassword) &&
                   !string.IsNullOrWhiteSpace(DbName) &&
                   !string.IsNullOrWhiteSpace(MailServer) &&
                   !string.IsNullOrWhiteSpace(MailPort) &&
                   !string.IsNullOrWhiteSpace(MailUser) &&
                   !string.IsNullOrWhiteSpace(MailPassword);
        }
    }
}
