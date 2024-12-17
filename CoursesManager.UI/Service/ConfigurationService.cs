
using System.IO;
using CoursesManager.MVVM.Env;
using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;


namespace CoursesManager.UI.Service
{
    public class ConfigurationService : IConfigurationService
    {

        private readonly EncryptionService _encryptionService;


        public ConfigurationService(EncryptionService encryptionService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        public void SaveEnvSettings(Dictionary<string, string> dbParams, Dictionary<string, string> mailParams)
        {
            Console.WriteLine("Encryptie van instellingen gestart...");

            var dbConnectionString = BuildConnectionString(dbParams);
            var mailConnectionString = BuildConnectionString(mailParams);



            EnvManager<EnvModel>.Values.ConnectionString = _encryptionService.Encrypt(dbConnectionString);
            EnvManager<EnvModel>.Values.MailConnectionString = _encryptionService.Encrypt(mailConnectionString);



            EnvManager<EnvModel>.Save();

            EnvManager<EnvModel>.Values.ConnectionString = _encryptionService.Decrypt(EnvManager<EnvModel>.Values.ConnectionString);
            EnvManager<EnvModel>.Values.MailConnectionString = _encryptionService.Decrypt(EnvManager<EnvModel>.Values.MailConnectionString);

        }


        public string SafeDecrypt(string encryptedText)
        {
            try
            {
                return _encryptionService.Decrypt(encryptedText);
            }
            catch
            {
                Console.WriteLine("Fout bij decryptie. Mogelijk onjuiste sleutel of waarde niet versleuteld.");
                return encryptedText;
            }
        }


        public EnvModel GetDecryptedEnvSettings()
        {
            EnvManager<EnvModel>.Values.ConnectionString =
                SafeDecrypt(EnvManager<EnvModel>.Values.ConnectionString);

            EnvManager<EnvModel>.Values.MailConnectionString =
                SafeDecrypt(EnvManager<EnvModel>.Values.MailConnectionString);

            return EnvManager<EnvModel>.Values;
        }





        public Dictionary<string, string> GetDatabaseParameters()
        {
            var decryptedConfig = GetDecryptedEnvSettings();
            if (string.IsNullOrWhiteSpace(decryptedConfig.ConnectionString))
            {
                Console.WriteLine("Geen ConnectionString gevonden in de gedecrypteerde instellingen.");
                return new Dictionary<string, string>();
            }

            return ParseConnectionString(decryptedConfig.ConnectionString);
        }

        public Dictionary<string, string> GetMailParameters()
        {
            var decryptedConfig = GetDecryptedEnvSettings();
            if (string.IsNullOrWhiteSpace(decryptedConfig.MailConnectionString))
            {
                Console.WriteLine("Geen MailConnectionString gevonden in de gedecrypteerde instellingen.");
                return new Dictionary<string, string>();
            }

            return ParseConnectionString(decryptedConfig.MailConnectionString);
        }



        public bool ValidateSettings()
        {

            var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (!File.Exists(envFilePath))
            {
                Console.WriteLine("Geen .env-bestand gevonden. Gebruiker moet instellingen invullen.");
                return false;
            }


            try
            {
                var config = GetDecryptedEnvSettings();


                if (string.IsNullOrWhiteSpace(config.ConnectionString) ||
                    string.IsNullOrWhiteSpace(config.MailConnectionString))
                {
                    Console.WriteLine("De ConnectionString of MailConnectionString is leeg of ongeldig.");
                    return false;
                }


                if (!ValidateConnectionString(config.ConnectionString))
                {
                    Console.WriteLine("De ConnectionString is ongeldig.");
                    return false;
                }

                using (var connection = new MySqlConnection(config.ConnectionString))
                {
                    connection.Open();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij het valideren van instellingen: {ex.Message}");
                return false;
            }
        }

        private bool ValidateConnectionString(string connectionString)
        {
            return connectionString.Contains("Server=") &&
                   connectionString.Contains("Database=") &&
                   connectionString.Contains("User=") &&
                   connectionString.Contains("Password=");
        }



        private Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("ConnectionString is leeg of null.");
                return new Dictionary<string, string>();
            }

            try
            {
                var result = connectionString
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Where(part => part.Contains("="))
                    .Select(part => part.Split('='))
                    .ToDictionary(split => split[0].Trim(), split => split[1].Trim());

                foreach (var kvp in result)
                {
                    Console.WriteLine($"Parsed: {kvp.Key} = {kvp.Value}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij het parsen van de connectionstring: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }


        private string BuildConnectionString(Dictionary<string, string> parameters)
        {
            return string.Join(";", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
        }

    }
}
