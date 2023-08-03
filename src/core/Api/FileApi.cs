using System;
using System.IO;
using Nancy;

namespace SM64DSe.core.Api
{
    public class FileApi : NancyModule
    {
        private static readonly string[] ALLOWED_EXTENSION = new string[] { ".mtl", ".obj", ".png" };

        private static bool IsFileAllowed(string name)
        {
            foreach (var s in ALLOWED_EXTENSION)
            {
                if (name.EndsWith(s))
                    return true;
            }

            return false;
        }
        
        
        public FileApi() : base("/api/files")
        {
            Get("/", _ => Response.AsJson(Program.romEditor.GetManager<FileManager>().GetFileEntries()));
            Get("/directories", _ => Response.AsJson(Program.romEditor.GetManager<FileManager>().GetDirEntries()));
            /*Get("/{fileId:int}/details",
                args => Response.AsJson(Program.romEditor.GetManager<FileManager>().GetFileFromInternalID((ushort)args.fileId)));*/

            Get("/{file}",
                args =>
                {
                    // Split the file request between filename and extension (if exist)
                    string[] arr = args.file.ToString().Split('.');
                    int fileId = -1;
                    // Check if the file provided is a number
                    bool canConvert = int.TryParse(arr[0], out fileId);

                    // If the query parameter "internal-id" is provided, we need to convert the id provided to fileId
                    string isInternalIdQuery = Request.Query["internal-id"];
                    string narcIdQuery = Request.Query["narc-id"];

                    // internal-id and narc-id are mutually exclusive
                    if (!string.IsNullOrEmpty(isInternalIdQuery) && !string.IsNullOrEmpty(narcIdQuery))
                        throw new Exception("internal-id and narc-id cannot be both set.");
                    
                    bool isInternalId = !string.IsNullOrEmpty(isInternalIdQuery) && bool.Parse(isInternalIdQuery);

                    // if not extension
                    if (arr.Length == 1)
                    {
                        if (!canConvert)
                            throw new Exception("without extension, only fileId are supported.");

                        if (!string.IsNullOrEmpty(narcIdQuery))
                            return Response.AsJson(
                                Program.romEditor.GetManager<FileManager>()
                                    .GetNARCFile((ushort)fileId, ushort.Parse(narcIdQuery))
                                    .ToFileDetails()
                            );

                        return Response.AsJson(
                            Program.romEditor.GetManager<FileManager>().GetFileEntry(
                                isInternalId
                                    ? Program.romEditor.GetManager<FileManager>().InternalIdToFileId((ushort)fileId)
                                    : (ushort)fileId
                            ).toFileDetails()
                        );
                    }

                    // Ensure the file requested as an allowed extension
                    if (!IsFileAllowed(args.file))
                        throw new Exception("File extension not allowed.");

                    var root = Program.romEditor.CurrentTemp;
                    string expectedFilename;
                    if (canConvert && isInternalId)
                    {
                        fileId = Program.romEditor.GetManager<FileManager>().InternalIdToFileId((ushort)fileId);
                        expectedFilename =
                            $"{fileId}.{arr[1]}";
                    }
                    else if(!string.IsNullOrEmpty(narcIdQuery))
                    {
                        expectedFilename = $"{narcIdQuery}-{args.file}";
                    }
                    else
                    {
                        expectedFilename = args.file;
                    }
                    
                    string path = Path.Combine(root, expectedFilename);
                    if (File.Exists(path))
                        return BaseApi.Serve(path, expectedFilename);
                    

                    // Depending on the extension requested we have different behavior
                    switch (arr[1])
                    {
                        case "obj":
                        case "mtl":
                            if (!canConvert)
                                throw new Exception("Only fileId can be converted to obj/mtl");

                            bool success;
                            if (!string.IsNullOrEmpty(narcIdQuery))
                            {
                                success = Program.romEditor.GetManager<ConverterManager>().ConvertBMDToObj(
                                    ushort.Parse(narcIdQuery),
                                    (ushort)fileId, 
                                    Path.Combine(root, $"{narcIdQuery}-{fileId}.obj")
                                );
                            }
                            else
                            {
                               success = Program.romEditor.GetManager<ConverterManager>().ConvertBMDToObj(
                                   (ushort)fileId, 
                                   Path.Combine(root, $"{fileId}.obj")
                               ); 
                            }

                            if (success)
                                return BaseApi.Serve(path, expectedFilename);
                            else
                                return new NotFoundResponse();
                            break;
                            default:
                                return new NotFoundResponse();
                    }
                });
            
            
            Get("/search",
                args =>
                {
                    string path = this.Request.Query["path"];
                    if (path != null)
                        return Response.AsJson(Program.romEditor.GetManager<FileManager>().GetFileFromName(path).ToFileDetails());
                    
                    return new NotFoundResponse();
                });

        }
    }
}