
namespace SM64DSe.core.Api
{
    public abstract class Manager
    {
        protected NitroROM m_ROM;
        
        protected Manager(NitroROM m_ROM)
        {
            this.m_ROM = m_ROM;
        }
    }
}