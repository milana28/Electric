using System.Collections.Generic;
using System.IO;
using Enclosure = Electric.Models.Enclosure;

namespace Electric.Pdf
{
    public static class TemplateGenerator
    {
        public static string GetHtmlString(List<Enclosure> enclosures)
        {
            var content = File.ReadAllText("Assets/pdf.html");
            return content;
        }
    }
}

