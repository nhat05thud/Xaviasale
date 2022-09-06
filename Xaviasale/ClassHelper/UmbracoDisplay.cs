using System.ComponentModel;
namespace Xaviasale.ClassHelper
{
    public class UmbracoDisplay: DisplayNameAttribute
    {
        private string DisplayNameKey { get; set; }
        private string ResourceSetName { get; set; }

        public UmbracoDisplay(string displayNameKey)
            : base(displayNameKey)
        {
            this.DisplayNameKey = displayNameKey;
        }


        public UmbracoDisplay(string displayNameKey, string resourceSetName)
            : base(displayNameKey)
        {
            this.DisplayNameKey = displayNameKey;
            this.ResourceSetName = resourceSetName;
        }

        public override string DisplayName
        {
            get
            {
                return UmbracoHelper.GetDictionaryItem(DisplayNameKey);
            }
        }
    }
}