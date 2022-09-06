using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using LightMvcCaptcha.Core;
namespace TranLeUmbraco
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UmbracoCaptchaAttribute: ValidationAttribute
    {
        private string _sessionName;
        public UmbracoCaptchaAttribute(string msgkey, string sessionName)
        {
            ErrorMessage = UmbracoHelper.GetDictionaryItem(msgkey);
            _sessionName = sessionName;
        }
        public override bool IsValid(object value)
        {
            string key = value as string;
            Captcha captcha = HttpContext.Current.Session[_sessionName] as Captcha;

            bool result = key != null && captcha != null && string.Equals(captcha.Key, key, Captcha.Comparison);

            HttpContext.Current.Session[_sessionName] = null;

            return result;
        }
    }
}