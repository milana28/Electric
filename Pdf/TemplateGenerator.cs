using System.Collections.Generic;
using System.Text;
using Enclosure = Electric.Models.Enclosure;

namespace Electric.Pdf
{
    public class TemplateGenerator
    {
        public static string GetHtmlString(List<Enclosure> enclosures)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <div class='header'><h1>ENCLOSURES</h1></div>
                                <table align='center'>
                                    <tr>
                                        <th>Name</th>
                                        <th>Date</th>
                                        <th>ProjectId</th>
                                        <th>Total price</th>
                                    </tr>");

            foreach (var en in enclosures)
            {
                sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    </tr>", en.Name, en.Date, en.ProjectId, en.TotalPrice);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }
    }
}

   