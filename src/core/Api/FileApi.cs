using Nancy;

namespace SM64DSe.core.Api
{
    public class FileApi: NancyModule
    {
        public FileApi(): base("/api/files")
        {
            Get("/", _ => Response.AsJson(Program.romEditor.GetManager<FileManager>().GetFileEntries()));
            Get("/directories", _ => Response.AsJson(Program.romEditor.GetManager<FileManager>().GetDirEntries()));
        }
    }
}