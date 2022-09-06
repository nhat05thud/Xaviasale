using LightMvcCaptcha.Core;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace TranLeUmbraco
{
    public static class CaptchaExtensions
    {
        /// <summary>
        /// Creates captcha img tag
        /// </summary>
        /// <param name="expression">Property with CaptchaAttribute</param>
        /// <param name="htmlAttributes">HTML attributes that will be applied to generated html code</param>
        /// <returns></returns>
        public static MvcHtmlString CaptchaFor<TModel, TValue>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression, string controllerName, string sessionKeyName, object htmlAttributes = null)
        {
            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
                throw new InvalidOperationException("Expression must be a member expression");

            bool usesCaptchaAttribute = memberExpression.Member.GetCustomAttributes(typeof(UmbracoCaptchaAttribute), true).Any();

            if (!usesCaptchaAttribute)
                throw new InvalidOperationException("Expression member must be with CaptchaAttribute");
            

            var urlHelper = new UrlHelper(html.ViewContext.RequestContext);
            var url = urlHelper.RouteUrl(new { controller = controllerName, action = "GetCaptcha", key = sessionKeyName });

            TagBuilder tag = new TagBuilder("img");

            if (htmlAttributes != null) tag.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            tag.Attributes.Add("src", url);
            tag.Attributes.Add("onclick", "this.src+=''");

            return MvcHtmlString.Create(tag.ToString(TagRenderMode.SelfClosing));
        }
    }
}