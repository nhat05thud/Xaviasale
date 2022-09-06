using Xaviasale.ClassHelper;

namespace Xaviasale.Models
{
    public class ContactModel : BaseModel
    {
        [UmbracoRequired("FormField.Name.Required")]
        public string Name { get; set; }
        [UmbracoRequired("FormField.Email.Required")]
        [UmbracoEmail("FormField.Email.Validation")]
        public string Email { get; set; }
        [UmbracoRequired("FormField.Phone.Required")]
        public string Phone { get; set; }
        public string IssueType { get; set; }
        public string ProductLink { get; set; }
        public string OrderNumber { get; set; }
        [UmbracoRequired("FormField.Message.Required")]
        public string Message { get; set; }
    }
}