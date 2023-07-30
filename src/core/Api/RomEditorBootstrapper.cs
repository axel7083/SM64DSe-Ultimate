using System;
using System.Collections;
using System.Reflection;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Configuration;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenTK;

namespace SM64DSe.core.Api
{
    /**
     * The Vector3 class of OpenTK is not properly serialized by Newtonsoft.Json
     * The following code fix it, by preventing to serialize anything other than X Y Z property in this class.
     */
    public class CustomJsonSerializer : JsonSerializer
    {
        public CustomJsonSerializer()
        {
            this.ContractResolver = new SM64DSContractResolver();
        }
    }
    public class SM64DSContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // Ignore serialization of fields other than X, Y, and Z in the Vector3 struct
            if (member.DeclaringType == typeof(Vector3) && !((IList)new[] { "X", "Y", "Z" }).Contains(property.PropertyName))
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }
    
    public class RomEditorBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT,OPTIONS");
                ctx.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Accept,Origin,Content-type,MY_HEADER");
                ctx.Response.Headers.Add("Access-Control-Expose-Headers", "Accept,Origin,Content-type,MY_HEADER");
            });
            base.ApplicationStartup(container, pipelines);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register<JsonSerializer, CustomJsonSerializer>();
            base.ConfigureApplicationContainer(container);
        }

        public override void Configure(INancyEnvironment environment)
        {
            environment.Tracing(true, true);
            base.Configure(environment);
        }
    }
}