namespace SM64DSe.core.Api
{
    public class LevelsManager : Manager
    {
        public LevelsManager(NitroROM m_ROM) : base(m_ROM)
        {
        }

        public string GetInternalLevelNameFromId(int id)
        {
            return m_ROM.GetInternalLevelNameFromID(id);
        }

        public ushort GetActSelectorIdByLevelId(int id)
        {
            return m_ROM.GetActSelectorIdByLevelID(id);
        }
    }
}