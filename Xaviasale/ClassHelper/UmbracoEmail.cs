using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Xaviasale.ClassHelper
{
    public class UmbracoEmail : RegularExpressionAttribute, IClientValidatable
    {
        public UmbracoEmail(string umbracoDictionaryKey) : base(GetRegex())
        {
            this.ErrorMessage = UmbracoHelper.GetDictionaryItem(umbracoDictionaryKey);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            yield return new ModelClientValidationRule
            {
                ErrorMessage = this.ErrorMessage,
                ValidationType = "email"
            };
        }
        private static string GetRegex()
        {
            // TODO: Go off and get your RegEx here
            return @"^[\w-]+(\.[\w-]+)*@([a-z0-9-]+(\.[a-z0-9-]+)*?\.[a-z]{2,6}|(\d{1,3}\.){3}\d{1,3})(:\d{4})?$";
        }
    }
}