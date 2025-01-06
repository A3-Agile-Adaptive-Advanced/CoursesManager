using CoursesManager.UI.Models;
using System.Text.RegularExpressions;

namespace CoursesManager.UI.Service.PlaceholderService
{
    /// <summary>
    /// Provides functionality for handling and validating placeholders within text.
    /// </summary>
    public class PlaceholderService : IPlaceholderService
    {
        /// <summary>
        /// A list of valid placeholders supported by the application.
        /// </summary>
        private readonly List<string> _validPlaceholders = new()
        {

            "[Cursus naam]",
            "[Cursus code]",
            "[Cursus beschrijving]",
            "[Cursus categorie]",
            "[Cursus startdatum]",
            "[Cursus einddatum]",
            "[Cursus locatie naam]",
            "[Cursus locatie land]",
            "[Cursus locatie postcode]",
            "[Cursus locatie stad]",
            "[Cursus locatie straat]",
            "[Cursus locatie huisnummer]",
            "[Cursus locatie toevoeging]",

            "[Betaal link]",

            "[Cursist naam]",
            "[Cursist email]",
            "[Cursist telefoonnummer]",
            "[Cursist geboortedatum]",
            "[Cursist adres land]",
            "[Cursist adres postcode]",
            "[Cursist adres stad]",
            "[Cursist adres straat]",
            "[Cursist adres huisnummer]",
            "[Cursist adres toevoeging]"
        };

        /// <summary>
        /// Retrieves the list of all valid placeholders.
        /// </summary>
        /// <returns>A list of valid placeholders.</returns>
        public List<string> GetValidPlaceholders() => _validPlaceholders;

        /// <summary>
        /// Validates placeholders in a given HTML string against the list of valid placeholders.
        /// </summary>
        /// <param name="htmlString">The HTML string containing placeholders to validate.</param>
        /// <returns>A list of invalid placeholders found in the provided HTML string.</returns>
        public List<string> ValidatePlaceholders(string htmlString)
        {
            var invalidPlaceholders = new List<string>();
            var regex = new Regex(@"\[(.*?)\]");
            var matches = regex.Matches(htmlString);

            foreach (Match match in matches)
            {
                if (!_validPlaceholders.Contains(match.Value))
                {
                    invalidPlaceholders.Add(match.Value);
                }
            }

            return invalidPlaceholders;
        }
    }

}
