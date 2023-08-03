using System.IO;
using Nancy;
using Nancy.Responses;
using Serilog;
using SM64DSe.core.Api.models;

namespace SM64DSe.core.Api
{
    public class BaseApi : NancyModule
    {
        public static Response Serve(string path, string filename)
        {
            Log.Information($"Serving {path}");
            var convertedFile = new FileStream(path, FileMode.Open);
            string fileName = filename;

            var response = new StreamResponse(() => convertedFile, MimeTypes.GetMimeType(path));
            return response.AsAttachment(fileName);
        }
        
        public BaseApi() : base("/api")
        {
            Get("/", _ => Response.AsJson(Program.romEditor.GetRomMetadata()));
            Get("/version", _ => Response.AsJson(new RomVersion(Program.romEditor.GetRomVersion().ToString())));
        }
    }
}