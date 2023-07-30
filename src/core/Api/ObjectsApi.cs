using Nancy;
using SM64DSe.core.models;

namespace SM64DSe.core.Api
{
    public class ObjectsApi : NancyModule
    {
        public ObjectsApi() : base("/api/objects")
        {
            Get("/{objectId:int}",
                args =>
                {
                    var param1 = this.Request.Query["parameter-1"];
                    var param2 = this.Request.Query["parameter-2"];
                    var param3 = this.Request.Query["parameter-3"];

                    var obj = new LevelObject(null, 0);
                    obj.ID = args.objectId;
                    obj.Parameters = new ushort[] { param1, param2, param3 };

                    return ObjectRenderer.FromLevelObject(obj).m_Filename;
                });
        }
    }
}