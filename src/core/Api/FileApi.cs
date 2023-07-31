using System;
using System.Diagnostics.Contracts;
using System.IO;
using Nancy;
using Nancy.Responses;
using Serilog;

namespace SM64DSe.core.Api
{
    public class FileApi : NancyModule
    {
        private static readonly string[] ALLOWED_EXTENSION = new string[] { ".mtl", ".obj", ".png" };
        
        private static Response Serve(string path, string filename)
        {
            Log.Information($"Serving {path}");
            var convertedFile = new FileStream(path, FileMode.Open);
            string fileName = filename;

            var response = new StreamResponse(() => convertedFile, MimeTypes.GetMimeType(path));
            return response.AsAttachment(fileName);
        }

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
            Get("/{fileId:int}/details",
                args => Response.AsJson(Program.romEditor.GetManager<FileManager>().GetFileFromInternalID((ushort)args.fileId)));

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
                    bool isInternalId = !string.IsNullOrEmpty(isInternalIdQuery) && bool.Parse(isInternalIdQuery);

                    // if not extension
                    if (arr.Length == 1)
                    {
                        if (!canConvert)
                            throw new Exception("without extension, only fileId are supported.");
                        
                        return Response.AsJson(
                            Program.romEditor.GetManager<FileManager>().GetFileEntry(
                                isInternalId?
                                    Program.romEditor.GetManager<FileManager>().InternalIdToFileId((ushort)fileId) :
                                    (ushort)fileId
                                    )
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
                    else
                    {
                        expectedFilename = args.file;
                    }
                    
                    string path = Path.Combine(root, expectedFilename);
                    if (File.Exists(path))
                        return Serve(path, expectedFilename);
                    

                    // Depending on the extension requested we have different behavior
                    switch (arr[1])
                    {
                        case "obj":
                        case "mtl":
                            if (!canConvert)
                                throw new Exception("Only fileId can be converted to obj/mtl");
                            
                            bool success = Program.romEditor.GetManager<ConverterManager>().ConvertBMDToObj(
                                (ushort)fileId, 
                                Path.Combine(root, $"{fileId}.obj")
                            );

                            if (success)
                                return Serve(path, expectedFilename);
                            else
                                return new NotFoundResponse();
                            break;
                            default:
                                return new NotFoundResponse();
                    }
                    
                    
                    /*string file = args.file;
                    
                    bool isInternalId = bool.Parse((internalId != null)?internalId:"false");

                    var arr = file.Split('.');
                    if (arr.Length != 2)
                    {
                        ushort id;
                        ushort.TryParse(arr[0], out id);
                        if (isInternalId)
                        {
                            id = Program.romEditor.GetManager<FileManager>().InternalIdToFileId(id);
                            Log.Information($"Converting internalID to FileID: {arr[0]} -> {id}");
                        }
                        return Response.AsJson(Program.romEditor.GetManager<FileManager>().GetFileEntry(id));
                    }

                    if (!IsFileAllowed(file))
                        throw new FormatException("The file request has not an acceptable extension.");

                    var root = Program.romEditor.CurrentTemp;
                    string path = Path.Combine(root, args.file);

                    if (File.Exists(path))
                        return Serve(path, file);
                    
                    switch (arr[1])
                    {
                        case "obj":
                        case "mtl":
                            ushort fileID = ushort.Parse(arr[0]);
                            if (isInternalId)
                            {
                                fileID = Program.romEditor.GetManager<FileManager>().InternalIdToFileId(fileID);
                                Log.Information($"Converting internalID to FileID: {arr[0]} -> {fileID}");
                            }
                            
                            Program.romEditor.GetManager<ConverterManager>().ConvertBMDToObj(
                                fileID, 
                                Path.Combine(root, $"{fileID}.obj")
                                );
                            return Serve(path, $"{fileID}.{arr[1]}");
                        default:
                            return new NotFoundResponse();
                    }*/
                });
        }
    }
}