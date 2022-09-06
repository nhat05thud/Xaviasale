using System.Web.Optimization;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.Dashboards;

namespace Xaviasale
{
    public class SectionComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Dashboards().Remove<RedirectUrlDashboard>();
            composition.Components().Append<BundleComponent>();
        }
        public class BundleComponent : IComponent
        {
            public void Initialize()
            {
                BundleConfig.RegisterBundles(BundleTable.Bundles);
            }

            public void Terminate()
            {
            }
        }
    }
}