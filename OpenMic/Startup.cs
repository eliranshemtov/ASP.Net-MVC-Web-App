using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OpenMic.Startup))]
namespace OpenMic
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
