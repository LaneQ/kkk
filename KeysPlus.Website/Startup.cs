using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(KeysPlus.Website.Startup))]
namespace KeysPlus.Website
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
