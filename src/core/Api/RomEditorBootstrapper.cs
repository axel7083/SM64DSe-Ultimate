using Nancy;
using Nancy.Configuration;

namespace SM64DSe.core.Api
{
    public class RomEditorBootstrapper : DefaultNancyBootstrapper
    {
        public override void Configure(INancyEnvironment environment)
        {
            environment.Tracing(true, true);
            base.Configure(environment);
        }
    }
}