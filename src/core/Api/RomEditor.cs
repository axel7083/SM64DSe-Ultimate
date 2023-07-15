using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using SM64DSe.Exceptions;

namespace SM64DSe.core.Api
{
    public class RomEditor
    {
        private NitroROM m_ROM;
        private RomMetadata metadata;
        public bool isOpen;
        
        private Dictionary<Type, Manager> managers = new Dictionary<Type, Manager>();

        public T GetManager<T>() where T : Manager
        {
            Type managerType = typeof(T);

            if (managers.ContainsKey(managerType))
                return (T)managers[managerType];
            
            if (!isOpen)
                throw new Exception($"Trying to access {managerType.Name} without any ROM opened.");

            Manager manager = (T)Activator.CreateInstance(managerType, m_ROM);
            managers[managerType] = manager;

            return (T)manager;
        }

        public RomEditor(string[] args)
        {
            if (args.Length >= 1) {
                if (args[0].EndsWith(".nds")) { 
                    LoadRom(args[0]);
                } else {
                    //TODO
                }
            }
        }

        public NitroROM DangerousGetRom()
        {
#if DEBUG
            Log.Warning(
                "WARNING: manually getting m_ROM\n" + 
                new System.Diagnostics.StackTrace().ToString()
            );
#endif
            return this.m_ROM;
        }

        public void Cleanup()
        {
            // Cleanup
            while (Program.m_LevelEditors.Count > 0)
                Program.m_LevelEditors[0].Close();
            m_ROM.EndRW();
            Program.m_ROMPath = "";
        }

        public void LoadRom(string filename)
        {
            Log.Information("Loading ROM from " + filename);
            // Check file exist
            if (!File.Exists(filename))
                throw new RomEditorException("The specified file doesn't exist.");

            if (isOpen)
                Cleanup();
            
            this.m_ROM = new NitroROM(filename);
            isOpen = true;
            
            Program.m_IsROMFolder = false;
            Program.m_ROMPath = filename;

            metadata = new RomMetadata(
                false, 
                null, 
                null, 
                null, 
                null, 
                filename
                );
            Log.Information("Rom version: " + GetRomVersion());
        }

        public void LoadTables()
        {
            m_ROM.LoadTables();
            m_ROM.EndRW();
        }

        public void BackupRom(string destination)
        {
            this.m_ROM.EndRW();
            File.Copy(Program.m_ROMPath, destination, true);
        }

        public NitroROM.Version GetRomVersion()
        {
            return m_ROM.m_Version;
        }
    }
}