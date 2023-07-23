using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using SM64DSe.core.Api;
using SM64DSe.core.models;
using SM64DSe.core.NSMBe4;
using SM64DSe.core.NSMBe4.DSFileSystem;

namespace SM64DSe.core.Editor.Managers
{
    public class CompilerManager : Manager
    {
        private readonly CompilationConfig configuration;


        public List<OverlayTarget> GetOverlayTargets()
        {
            return configuration.overlays;
        }

        public string getPatchFolder()
        {
            return configuration.patch != null ? configuration.patch.Path : "";
        }
        public CompilerManager(NitroROM m_ROM) : base(m_ROM)
        {
            // TODO: load from file.
            configuration = new CompilationConfig();
        }

        public void addOverlayTarget(OverlayTarget target)
        {
            configuration.overlays.Add(target);
        }
        
        public void removeOverlayTarget(OverlayTarget target)
        {
            configuration.overlays.Remove(target);
        }

        public void SetPatchTarget(string path)
        {
            configuration.patch = new CompilationTarget(path);
        }

        public CompilationStatus build(OverlayTarget target)
        {
            try
            {
                if (target.OverlayId < 0)
                    throw new Exception("Overlay " + target.OverlayId + " does not exist.");
                
                var count = m_ROM.getOverlayCount();
                if(target.OverlayId > count + 1)
                    throw new Exception("You targeted the overlay " + target.OverlayId + ". it is too high, it should be " + count);

                if (target.OverlayId == count + 1)
                {
                    Log.Warning("We need to add a new overlay.");
                    Program.romEditor.GetManager<OverlaysManager>().CreateNewOverlay((uint) target.RamAddress);
                }

                uint codeAddr = new NitroOverlay(m_ROM, (uint) target.OverlayId).GetRAMAddr();
                        
                Log.Information("Trying to compile " + target.Path + " in overlay " + target.OverlayId + ".");
                var pm = new NSMBe4.Patcher.PatchMaker(new DirectoryInfo(target.Path), codeAddr);
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
                new DirectoryInfo(configuration.patch.Path)
            );
            pm.restore();
            pm.compilePatch();
            pm.generatePatch();
            
            Log.Information("Modified version successfully patched.");
            
            return CompilationStatus.SUCCESS;
        }

        public void BuildAll()
        {
            configuration.overlays.Sort(((a, b) => a.OverlayId - b.OverlayId));
            configuration.overlays.ForEach(target =>
            {
                target.Status = build(target);
            });
            configuration.patch.Status = BuildInjectPatch();
        }
    }
}