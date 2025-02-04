﻿using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Enums;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Service;
using System.Windows.Input;

namespace CoursesManager.UI.ViewModels
{
    public class ConfigurationViewModel : ViewModelWithNavigation
    {
        private readonly IConfigurationService _configurationService;
        private readonly IMessageBroker _messageBroker;
        private readonly INavigationService _navigationService;

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

        public ICommand SaveCommand { get; }

        public ConfigurationViewModel(IConfigurationService configurationService, IMessageBroker messageBroker, INavigationService navigationService) : base(navigationService)
        {

            _configurationService = configurationService;
            _messageBroker = messageBroker;
            _navigationService = navigationService;


            InitializeSettings();
            SaveCommand = new RelayCommand(ValidateAndSave);

            if (!CanSave() || !_configurationService.ValidateSettings())
            {
                _messageBroker.Publish(new ToastNotificationMessage(
                    true,
                    "Instellingen zijn ongeldig!",
                    ToastType.Info));
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
        }

        public async void ValidateAndSave()
        {
            _messageBroker.Publish(new ToastNotificationMessage(
                true,
                "Opslaan",
                ToastType.Info, true));
            try
            {
                if (!CanSave())
                {
                    await Task.Delay(5000); // Simuleer validatiedelay
                    _messageBroker.Publish(new ToastNotificationMessage(
                        true,
                        "Instellingen zijn ongeldig. Controleer de ingevoerde waarden.",
                        ToastType.Error));
                    return; // stop de methode, want er zijn fouten.
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
                    _messageBroker.Publish(new ToastNotificationMessage(
                        true,
                        "Instellingen zijn ongeldig. Controleer de ingevoerde waarden.",
                        ToastType.Error));
                    INavigationService.CanNavigate = false;
                    return;
                }

                INavigationService.CanNavigate = true;

                _messageBroker.Publish(new ToastNotificationMessage(
                    true,
                    "Instellingen succesvol opgeslagen!",
                    ToastType.Confirmation));

                _navigationService.NavigateTo<CoursesManagerViewModel>();
            }
            catch (Exception ex)
            {
                // Log de fout voor de systeembeheerder
                LogUtil.Log($"Fout bij ValidateAndSave: {ex.Message}");

                // Toon een generieke foutmelding aan de gebruiker
                _messageBroker.Publish(new ToastNotificationMessage(
                    true,
                    "Er is een fout opgetreden. Neem contact op met de systeembeheerder.",
                    ToastType.Error));
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
