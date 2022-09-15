namespace Xaviasale.TemplateEngine
{
    public interface ITextTemplate
    {
        string TemplateDir { get; }

        string Render(string templateName, object model);
    }
}