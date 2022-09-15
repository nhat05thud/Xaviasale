using System;
using System.IO;
using System.Text;
using DotLiquid;

namespace Xaviasale.TemplateEngine
{
    public class DotLiquidTemplate: ITextTemplate
    {
        public DotLiquidTemplate(string templateDir)
        {
            TemplateDir = templateDir;
        }

        public string TemplateDir { get; }

        public string Render(string templateName, object model)
        {
            if (templateName?.Contains("\\") == true) throw new ArgumentException($"templateName {templateName} is invalid");

            var templateFile = Path.Combine(TemplateDir, $"{templateName}.html");
            var templateFileCulture = Path.Combine(TemplateDir, $"{templateName}.html");

            if (File.Exists(templateFileCulture))
            {
                templateFile = templateFileCulture;
            }
            else if (!File.Exists(templateFile))
            {
                throw new FileNotFoundException($"TemplateName {templateName} not found.");
            }

            var liquidTemplate = File.ReadAllText(templateFile, Encoding.UTF8);

            var template = Template.Parse(liquidTemplate);
            try
            {
                return template.Render(Hash.FromAnonymousObject(model));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}