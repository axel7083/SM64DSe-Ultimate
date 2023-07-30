using Nancy;

namespace SM64DSe.core.Api
{
    public class FileApi : NancyModule
    {
        public FileApi() : base("/api/files")
        {
            Get("/", _ => Response.AsJson(Program.romEditor.GetManager<FileManager>().GetFileEntries()));
            Get("/directories", _ => Response.AsJson(Program.romEditor.GetManager<FileManager>().GetDirEntries()));
            Get("/{fileId:int}/details",
                args => Response.AsJson(Program.romEditor.GetManager<FileManager>().GetFileFromId((ushort)args.fileId)));
            
            Get("/{fileId:int}.obj",
                args => Response.AsJson(Program.romEditor.GetManager<ConverterManager>().GetObjFileFromId((ushort)args.fileId)));
        }
    }
}