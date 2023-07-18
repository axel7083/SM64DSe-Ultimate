using Nancy;
using Serilog;
using SM64DSe.core.Api.models;

namespace SM64DSe.core.Api
{
    public class BaseApi : NancyModule
    {
        public BaseApi() : base("/api")
        {
            Get("/", _ => Response.AsJson(Program.romEditor.GetRomMetadata()));
            Get("/version", _ => Response.AsJson(new RomVersion(Program.romEditor.GetRomVersion().ToString())));
        }
    }
}