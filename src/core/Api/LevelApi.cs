using Nancy;
using SM64DSe.core.models;

namespace SM64DSe.core.Api
{
    public class LevelApi : NancyModule
    {
        public LevelApi() : base("/api/levels")
        {
            Get("/", _ => Response.AsJson(Strings.LevelNames()));
            Get("/{levelId:int}/details",
                args => Response.AsJson(Program.romEditor.GetManager<LevelsManager>().GetLevelDetails((int)args.levelId)));
            Get("/{levelId:int}/objects",
                args => Response.AsJson(new LevelObjects(Program.romEditor.GetManager<LevelsManager>().GetLevelObjects((int)args.levelId))));
        }
    }
}