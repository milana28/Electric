using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RazorLight;
using Enclosure = Electric.Models.Enclosure;

namespace Electric.Pdf
{
    public class TemplateGenerator
    {
        public async Task<string> GetHtmlString(List<Enclosure> enclosures)
        {
            var engine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(Enclosure)) 
                .UseMemoryCachingProvider().Build();

            var pdf = new Pages.Pdf(enclosures);
            
            var template = await File.ReadAllTextAsync("Pages/Pdf.cshtml");
            var result = await engine.CompileRenderStringAsync("pdf", template, pdf);
            
            return result;
        }
    }
}
