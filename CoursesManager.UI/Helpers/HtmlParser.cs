using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;

namespace CoursesManager.UI.Helpers
{
    public class HtmlParser
    {

        public string ConvertToHtml(string input)
        {
            var builder = new StringBuilder("<html><body>");

            foreach (var line in input.Split('\n'))
            {
                string formattedLine = line;

                formattedLine = System.Text.RegularExpressions.Regex.Replace(
                    formattedLine,
                    @"\*\*(.+?)\*\*",
                    "<b>$1</b>"
                );

                formattedLine = System.Text.RegularExpressions.Regex.Replace(
                    formattedLine,
                    @"\*(.+?)\*",
                    "<i>$1</i>"
                );

                builder.Append($"<p>{System.Net.WebUtility.HtmlEncode(formattedLine)}</p>");
            }
;
            Debug.WriteLine(HttpUtility.HtmlEncode(builder.ToString()));
            builder.Append("</body></html>");
            return builder.ToString();
        }

        public string ConvertFromHtml(string input)
        {
            string decodedInput = System.Net.WebUtility.HtmlDecode(input);

            decodedInput = System.Text.RegularExpressions.Regex.Replace(
                decodedInput,
                @"<b>(.+?)<\/b>",
                "**$1**",
                System.Text.RegularExpressions.RegexOptions.Singleline
            );

            decodedInput = System.Text.RegularExpressions.Regex.Replace(
                decodedInput,
                @"<strong>(.+?)<\/strong>",
                "**$1**",
                System.Text.RegularExpressions.RegexOptions.Singleline
            );

            decodedInput = System.Text.RegularExpressions.Regex.Replace(
                decodedInput,
                @"<i>(.+?)<\/i>",
                "*$1*",
                System.Text.RegularExpressions.RegexOptions.Singleline
            );

            var lines = System.Text.RegularExpressions.Regex.Replace(
                decodedInput,
                @"<\/?p>",
                string.Empty
            );

            return lines.Trim();
        }




    }
}
