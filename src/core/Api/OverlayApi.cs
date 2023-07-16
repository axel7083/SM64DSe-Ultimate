using Nancy;
using SM64DSe.core.models;

namespace SM64DSe.core.Api
{
    public class OverlayApi: NancyModule
    {
        public OverlayApi() : base("/api/overlays")
        {
            Get("/", _ =>
            {
                string ovlId = Request.Query["ovlId"];
                if (ovlId != null)
                    return Response.AsJson(Program.romEditor.GetManager<OverlaysManager>().GetOverlay(uint.Parse(ovlId)));
                
                string levelId = Request.Query["levelId"];
                if(levelId != null)
                    return Response.AsJson(Program.romEditor.GetManager<OverlaysManager>().GetLevelOverlayID(int.Parse(levelId)));
                
                return Response.AsJson(Program.romEditor.GetManager<OverlaysManager>().GetOverlayEntries());
            });
            Get("/count", _ => Response.AsJson(new OverlayCount(Program.romEditor.GetManager<OverlaysManager>().GetOverlayCount())));
            
            Get("/{ovlId:int}", args => Response.AsJson(Program.romEditor.GetManager<OverlaysManager>().GetOverlay((uint)args.ovlId)));
            
        }
    }
}