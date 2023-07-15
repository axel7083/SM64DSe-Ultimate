using SM64DSe.Exceptions;

namespace SM64DSe.core.Api
{
    public class PatchesManager: Manager
    {
        public PatchesManager(NitroROM m_ROM) : base(m_ROM) { }
        
        public bool NeedsPatch()
        {
            this.m_ROM.BeginRW();
            return this.m_ROM.NeedsPatch();
        }

        public void Patch()
        {
            // switch to buffered RW mode (faster for patching)
            this.m_ROM.BeginRW(true);
            this.m_ROM.Patch();
        }

        
        // Very ugly, should be deprecated
        public bool ToggleSuitabilityForNSMBeASMPatching()
        {
            if (Program.m_IsROMFolder)
                throw new RomEditorException("Cannot toggle suitability with extracted ROM.");
            
            m_ROM.BeginRW();

            bool suitable = (this.m_ROM.Read32(0x4AF4) == 0xDEC00621 && this.m_ROM.Read32(0x4AF8) == 0x2106C0DE) ? true : false;
            if (!suitable)
            {
                m_ROM.Write32(0x4AF4, 0xDEC00621);
                m_ROM.Write32(0x4AF8, 0x2106C0DE);
                uint binend = (Program.m_ROM.Read32(0x2C) + 0x4000);
                m_ROM.Write32(binend, 0xDEC00621);
                m_ROM.Write32(0x4AEC, 0x00000000);
            }
            else
            {
                m_ROM.Write32(0x4AF4, 0x00000000);
                m_ROM.Write32(0x4AF8, 0x00000000);
                uint binend = (Program.m_ROM.Read32(0x2C) + 0x4000);
                m_ROM.Write32(binend, 0x00000000);
                m_ROM.Write32(0x4AEC, 0x02061504);
            }

            m_ROM.EndRW();
            return suitable;
        }

        public bool IsDlPatch()
        {
            bool autorw = Program.m_ROM.CanRW();
            if (!autorw) Program.m_ROM.BeginRW();

            return Program.m_ROM.Read32(0x6590) != 0;
        }

        public void DlPatch()
        {
            Program.m_ROM.BeginRW();
            NitroOverlay ov2 = new NitroOverlay(Program.m_ROM, 2);

            //Move the ACTOR_SPAWN_TABLE so it can expand
            Program.m_ROM.WriteBlock(0x6590, Program.m_ROM.ReadBlock(0x90864, 0x61c));
            Program.m_ROM.WriteBlock(0x90864, new byte[0x61c]);

            //Adjust pointers
            Program.m_ROM.Write32(0x1a198, 0x02006590);

            //Move the OBJ_TO_ACTOR_TABLE so it can expand
            Program.m_ROM.WriteBlock(0x4b00, ov2.ReadBlock(0x0210cbf4 - ov2.GetRAMAddr(), 0x28c));
            ov2.WriteBlock(0x0210cbf4 - ov2.GetRAMAddr(), new byte[0x28c]);

            //Adjust pointers
            ov2.Write32(0x020fe890 - ov2.GetRAMAddr(), 0x02004b00);
            ov2.Write32(0x020fe958 - ov2.GetRAMAddr(), 0x02004b00);
            ov2.Write32(0x020fea44 - ov2.GetRAMAddr(), 0x02004b00);

            //Add the dynamic library loading and cleanup code
            Program.m_ROM.WriteBlock(0x90864, Properties.Resources.dynamic_library_loader);

            //Add the hooks (by replacing LoadObjBankOverlays())
            Program.m_ROM.WriteBlock(0x2df70, Properties.Resources.static_overlay_loader);

            Program.m_ROM.EndRW();
            ov2.SaveChanges();
        }
    }
}