namespace Xaviasale.ClassHelper
{
    public static class UmbracoHelper
    {
        public static string GetDictionaryItem(string key)
        {
            var helper = Umbraco.Web.Composing.Current.UmbracoHelper;
            string errorMessage = helper.GetDictionaryValue(key);
            if (string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = key + " is missing";
            }
            return errorMessage;

        }
    }
}