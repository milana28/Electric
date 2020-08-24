using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Enclosure = Electric.Models.Enclosure;

namespace Electric.Pages
{
    public class Pdf : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public List<Enclosure> Enclosures { get; }

        public Pdf(List<Enclosure> enclosures)
        {
            Enclosures = enclosures;
        }
    }
}