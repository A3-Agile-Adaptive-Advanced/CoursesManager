
using System.Security.Cryptography;


namespace CoursesManager.UI.Service
{
    public class EncryptionService
    {
        private string _key;

        public EncryptionService(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("de sleutel mag niet leeg zijn", nameof(key));
            }

            this._key = key;
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                throw new ArgumentException("de tekst om te versleutelen mag niet leeg zijn", nameof(plainText));
            }

            using (var aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(_key);
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                    var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
            {
                throw new ArgumentException("De tekst om te ontsleutelen mag niet leeg zijn.", nameof(encryptedText));
            }

            
            var parts = encryptedText.Split(':');
            if (parts.Length != 2)
            {
                Console.WriteLine("Waarschuwing: De tekst lijkt niet versleuteld te zijn. Originele tekst wordt geretourneerd.");
                return encryptedText; 
            }

            try
            {
                var iv = Convert.FromBase64String(parts[0]);
                var encryptedBytes = Convert.FromBase64String(parts[1]);

                using (var aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String(_key);
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        return System.Text.Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij het ontsleutelen: {ex.Message}");
                throw;
            }
        }
    }
}
