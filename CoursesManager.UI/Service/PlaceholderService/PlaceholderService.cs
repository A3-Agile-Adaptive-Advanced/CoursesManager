using CoursesManager.UI.Models;
using System.Text.RegularExpressions;

namespace CoursesManager.UI.Service.PlaceholderService
{

    public class PlaceholderService : IPlaceholderService
    {
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

        public List<string> GetValidPlaceholders() => _validPlaceholders;

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
