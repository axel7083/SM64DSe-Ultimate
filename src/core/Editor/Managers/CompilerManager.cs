using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Serilog;
using SM64DSe.core.Api;
using SM64DSe.core.models;
using SM64DSe.core.NSMBe4;
using SM64DSe.core.NSMBe4.DSFileSystem;

namespace SM64DSe.core.Editor.Managers
{
    public class CompilerManager : Manager
    {
        private CompilationConfig configuration;
        private string configPath = null;


        public List<OverlayTarget> GetOverlayTargets()
        {
            return configuration.Overlays;
        }

        public string getPatchFolder()
        {
            return configuration.Patch != null ? configuration.Patch.Path : "";
        }
        public CompilerManager(NitroROM m_ROM) : base(m_ROM)
        {
            // TODO: load from file.
            configuration = new CompilationConfig();
        }

        public void LoadConfig(string filename)
        {
            configPath = filename;
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                configuration = (CompilationConfig)serializer.Deserialize(file, typeof(CompilationConfig));
            }

            validatePath();
        }

        private string RealPath(CompilationTarget target)
        {
            if (Directory.Exists(target.Path))
                return target.Path;

            var nPath = Path.Combine(new DirectoryInfo(configPath).Parent.FullName, target.Path);
            if (Directory.Exists(nPath))
                return nPath;

            throw new Exception("The path " + target.Path + " provided, is neither a relative or absolute path.");
        }

        private void validatePath()
        {
            RealPath(configuration.Patch);
            configuration.Overlays.ForEach(target =>
            {
                RealPath(target);
            });
        }

        public void addOverlayTarget(OverlayTarget target)
        {
            configuration.Overlays.Add(target);
        }
        
        public void removeOverlayTarget(OverlayTarget target)
        {
            configuration.Overlays.Remove(target);
        }

        public void SetPatchTarget(string path)
        {
            configuration.Patch = new CompilationTarget(path);
        }

        public CompilationStatus build(OverlayTarget target)
        {
            try
            {
                if (target.OverlayId < 0)
                    throw new Exception("Overlay " + target.OverlayId + " does not exist.");
                
                var count = m_ROM.getOverlayCount();
                if(target.OverlayId > count)
                    throw new Exception("You targeted the overlay " + target.OverlayId + ". it is too high, it should be " + count);

                if (target.OverlayId == count)
                {
                    Log.Warning("We need to add a new overlay.");
                    Program.romEditor.GetManager<OverlaysManager>().CreateNewOverlay((uint) target.RamAddress);
                }

                var realPath = RealPath(target);
                uint codeAddr = new NitroOverlay(m_ROM, (uint) target.OverlayId).GetRAMAddr();
                        
                Log.Information("Trying to compile " + realPath + " in overlay " + target.OverlayId + ".");
                var pm = new NSMBe4.Patcher.PatchMaker(new DirectoryInfo(realPath), codeAddr);
                pm.compilePatch();
                pm.MakeOverlay((uint) target.OverlayId);
                m_ROM.EndRW();
                
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Log.Error(target.Path + " failed to compiled.");
                return CompilationStatus.FAILED;
            }
            return CompilationStatus.SUCCESS;
        }

        public CompilationStatus BuildInjectPatch()
        {
            string backup = Path.Combine(new DirectoryInfo(m_ROM.path).Parent.FullName, "modified.nds");
            Log.Information("Creating modified version at " + backup + ".");
            Program.romEditor.BackupRom(backup);

            NitroROMFilesystem fs = new NitroROMFilesystem(backup);
            NSMBe4ROM.load(fs);
            SM64DSe.core.NSMBe4.Patcher.PatchMaker pm = new SM64DSe.core.NSMBe4.Patcher.PatchMaker(
                new DirectoryInfo(RealPath(configuration.Patch))
            );
            pm.restore();
            pm.compilePatch();
            pm.generatePatch();
            
            Log.Information("Modified version successfully patched.");
            
            return CompilationStatus.SUCCESS;
        }

        public void BuildAll()
        {
            configuration.Overlays.Sort(((a, b) => a.OverlayId - b.OverlayId));

            foreach (var target in configuration.Overlays)
            {
                target.Status = build(target);
                if (target.Status != CompilationStatus.SUCCESS)
                    return;
            }
            configuration.Patch.Status = BuildInjectPatch();
        }
    }
}