using System.Web.Http;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using Umbraco.Web;
using Xaviasale;

[assembly: OwinStartup("UmbracoStandardOwinStartup", typeof(UmbracoStandardOwinStartup))]
namespace Xaviasale
{
    public class UmbracoStandardOwinStartup : UmbracoDefaultOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            //ensure the default options are configured
            base.Configuration(app);

            // Configure hangfire
            var options = new SqlServerStorageOptions { PrepareSchemaIfNecessary = true };
            const string umbracoConnectionName = Umbraco.Core.Constants.System.UmbracoConnectionName;
            var connectionString = System.Configuration
                .ConfigurationManager
                .ConnectionStrings[umbracoConnectionName]
                .ConnectionString;

            Hangfire.GlobalConfiguration.Configuration
                .UseSqlServerStorage(connectionString, options);

            // Give hangfire a URL and start the server                
            var dashboardOptions = new DashboardOptions { Authorization = new[] { new UmbracoAuthorizationFilter() } };
            app.UseHangfireDashboard("/hangfire", dashboardOptions);
            app.UseHangfireServer();
        }
    }
}