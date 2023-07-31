using Nancy;
using SM64DSe.core.models;

namespace SM64DSe.core.Api
{
    public class ObjectsApi : NancyModule
    {
        public ObjectsApi() : base("/api/objects")
        {
            Get("/{objectId:int}",
                args => Response.AsJson(ObjectDatabase.m_ObjectInfo[args.objectId]));
        }
    }
}