
using System.Security.Cryptography;


namespace CoursesManager.UI.Service
{
    public class EncryptionService
    {
        private string _key;

        public EncryptionService(string key)
        {
            // zorgen dat de sleutel niet leeg is, want AES heeft de sleutel nodig
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("de sleutel mag niet leeg zijn", nameof(key));
            }

            this._key = key;
        }

        /// <summary>
        /// versleutelt een tekst met AES en retourneert een string met het IV en de versleutelde inhoud.
        /// </summary>
        /// <param name="plainText">de tekst die versleuteld moet worden.</param>
        /// <returns>een string met IV en de versleutelde tekst, gescheiden door een dubbele punt.</returns>

        public string Encrypt(string plainText)
        {
            
            if (string.IsNullOrWhiteSpace(plainText))
            {
                throw new ArgumentException("de tekst om te versleutelen mag niet leeg zijn", nameof(plainText));
            }

            using (var aes = Aes.Create())
            {
                // stel de sleutel in en genereer een unieke IV voor elke encryptie
                aes.Key = Convert.FromBase64String(_key);
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                    var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    // het combineren van de IV en de versleutelde tekst in 1 string
                    return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        /// <summary>
        /// ontsleutelt een versleutelde tekst met AES.
        /// </summary>
        /// <param name="encryptedText">de versleutelde tekst in het formaat "IV:encryptedData".</param>
        /// <returns>de originele (ontsleutelde) tekst.</returns>
        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
            {
                throw new ArgumentException("De tekst om te ontsleutelen mag niet leeg zijn.", nameof(encryptedText));
            }

            // splits de string in IV en versleutelde data
            var parts = encryptedText.Split(':');
            if (parts.Length != 2)
            {
                Console.WriteLine("Waarschuwing: De tekst lijkt niet versleuteld te zijn. Originele tekst wordt geretourneerd.");
                return encryptedText; 
            }

            try
            {
                var iv = Convert.FromBase64String(parts[0]); // haal de IV op
                var encryptedBytes = Convert.FromBase64String(parts[1]); // haal de versleutelde data op

                using (var aes = Aes.Create())
                {
                    // stel de sleutel en IV in
                    aes.Key = Convert.FromBase64String(_key);
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        // ontsleutel de data en converteert het naar de originele tekst
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
