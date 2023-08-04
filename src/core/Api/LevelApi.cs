using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;
using SM64DSe.core.models;

namespace SM64DSe.core.Api
{
    public class LevelApi : NancyModule
    {
        public LevelApi() : base("/api/levels")
        {
            Get("/", _ => Response.AsJson(Strings.LevelNames()));
            Get("/{levelId:int}/details",
                args => Response.AsJson(Program.romEditor.GetManager<LevelsManager>().GetLevel((int)args.levelId)));
            Get("/{levelId:int}/objects",
                args => Response.AsJson(new LevelObjects(Program.romEditor.GetManager<LevelsManager>().GetLevelObjects((int)args.levelId))));
            Post("/{levelId:int}/objects", async (args, ct) =>
            {
                LevelObjects levelObjects = this.Bind<LevelObjects>();
                if (levelObjects.objects == null || levelObjects.objects.Length == 0)
                    return new NotAcceptableResponse();
                
                Level level = Program.romEditor.GetManager<LevelsManager>().GetLevel((int)args.levelId);
                
                // TODO: replace levelObjects with the new one
                
                return "";
            });
        }
    }
}