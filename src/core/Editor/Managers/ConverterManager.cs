using System;
using System.IO;
using SM64DSe.ImportExport;

namespace SM64DSe.core.Api
{
    public class ConverterManager : Manager
    {
        public ConverterManager(NitroROM m_ROM) : base(m_ROM)
        {
            
        }

        public string GetObjFileFromId(ushort id)
        {
            var root = Program.romEditor.CurrentTemp;

            var path = Path.Combine(root, $"{id}.obj");
            if (File.Exists(path))
                return path;

            NitroFile file = Program.romEditor.GetManager<FileManager>().GetFileFromId(id);
            if (file.m_Name.EndsWith(".bmd"))
            {
                BMD_BCA_KCLExporter.ExportBMDModel(new BMD(file), path);
            }
            else
            {
                throw new Exception("Not an BMD file: " + file.m_Name);
            }

            return path;
        }
    }
}