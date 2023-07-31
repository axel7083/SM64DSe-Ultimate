using System;
using System.IO;
using Nancy;
using Nancy.Responses;
using Serilog;
using SM64DSe.ImportExport;

namespace SM64DSe.core.Api
{
    public class ConverterManager : Manager
    {
        public ConverterManager(NitroROM m_ROM) : base(m_ROM)
        {
            
        }

        public ushort GetFileIDFromInternalId(ushort id)
        {
            return m_ROM.GetFileIDFromInternalID(id);
        }

        public bool ConvertBMDToObj(ushort id, string path)
        {
            NitroFile file = Program.romEditor.GetManager<FileManager>().GetFileFromID(id);
            
            // Ensure it is a file that can be converted.
            if (!file.m_Name.EndsWith(".bmd"))
            {
                Log.Warning("Not an BMD file: " + file.m_Name);
                return false;
            }

                // Export the model to temp path.
            BMD_BCA_KCLExporter.ExportBMDModel(new BMD(file), path);
            return true;
        }
    }
}