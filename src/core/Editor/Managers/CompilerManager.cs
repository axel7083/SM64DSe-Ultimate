using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using SM64DSe.core.models;
using SM64DSe.core.NSMBe4;
using SM64DSe.core.NSMBe4.DSFileSystem;

namespace SM64DSe.core.Api
{
    public class CompilerManager : Manager
    {
        public List<CompilerTarget> targets { get; }


        public CompilerManager(NitroROM m_ROM) : base(m_ROM)
        {
            targets = new List<CompilerTarget>(); //TODO: load from somewhere
        }

        public void addTarget(CompilerTarget target)
        {
            targets.Add(target);
        }
        
        public void removeTarget(CompilerTarget target)
        {
            targets.Remove(target);
        }

        public void build()
        {
            targets.ForEach(target =>
            {
                try
                {
                    if (target.OverlayId >= m_ROM.getOverlayCount())
                        throw new Exception("Overlay " + target.OverlayId + " does not exist.");
                    
                    uint codeAddr = new NitroOverlay(m_ROM, target.OverlayId).GetRAMAddr();
                    
                    Log.Information("Trying to compile " + target.Path + ".");
                    var pm = new NSMBe4.Patcher.PatchMaker(new DirectoryInfo(target.Path), codeAddr);
                    pm.compilePatch();
                    pm.MakeOverlay(target.OverlayId);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Log.Error(target.Path + " failed to compiled.");
                    target.Status = CompilationStatus.FAILED;
                    return;
                }
                
                target.Status = CompilationStatus.SUCCESS;
            });
        }
    }
}