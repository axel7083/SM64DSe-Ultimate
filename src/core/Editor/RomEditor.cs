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

        public string CurrentTemp;

        private Dictionary<Type, Manager> managers = new Dictionary<Type, Manager>();

        public T GetManager<T>() where T : Manager
        {
            var managerType = typeof(T);

            if (managers.ContainsKey(managerType))
                return (T)managers[managerType];

            if (!isOpen)
                throw new Exception($"Trying to access {managerType.Name} without any ROM opened.");

            Manager manager = (T)Activator.CreateInstance(managerType, m_ROM);
            managers[managerType] = manager;

            return (T)manager;
        }

        public void ParseArguments(string[] args)
        {
            if (args.Length >= 1)
            {
                if (args[0].EndsWith(".nds"))
                {
                    LoadRom(args[0]);
                }
                else
                {
                    //TODO
                }
            }
        }

        public RomMetadata GetRomMetadata()
        {
            return metadata;
        }

        public NitroROM DangerousGetRom()
        {
            return m_ROM;
        }

        public void Cleanup()
        {
            // Cleanup
            while (Program.m_LevelEditors.Count > 0)
                Program.m_LevelEditors[0].Close();
            m_ROM.EndRW();
            metadata = null;
            isOpen = false;

            if (CurrentTemp != null && Directory.Exists(CurrentTemp))
            {
                Log.Warning("Cleaning temp folder " + CurrentTemp + ".");
                Directory.Delete(CurrentTemp);  
            }
        }

        public void LoadRom(string filename)
        {
            Log.Information("Loading ROM from " + filename);
            if (isOpen)
                Cleanup();

            metadata = new RomMetadata(
                false,
                null,
                null,
                null,
                null,
                filename
            );

            // Check file exist
            if (!File.Exists(filename))
                throw new RomEditorException("The specified file doesn't exist.");

            m_ROM = new NitroROM(filename);
            isOpen = true;

            CurrentTemp = GetTemporaryDirectory();

            Log.Information("Rom version: " + GetRomVersion());
            Log.Information("Creating Temp folder in " + CurrentTemp + ".");
        }

        public void LoadRomExtracted()
        {
            //TODO
        }
        
        private string GetTemporaryDirectory()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public void LoadTables()
        {
            m_ROM.BeginRW();
            m_ROM.LoadTables();
            m_ROM.EndRW();
        }

        public void BackupRom(string destination)
        {
            m_ROM.EndRW();
            File.Copy(Program.m_ROMPath, destination, true);
        }

        public NitroROM.Version GetRomVersion()
        {
            return m_ROM.m_Version;
        }
    }
}