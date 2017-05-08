using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MSProjectIntegrator.Startup))]
namespace MSProjectIntegrator
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
