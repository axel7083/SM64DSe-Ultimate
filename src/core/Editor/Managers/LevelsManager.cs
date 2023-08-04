using System.Collections.Generic;
using System.Linq;
using Serilog;

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

        public Level GetLevel(int id)
        {
            return new Level(id);
        }
        
        public LevelObject[] GetLevelObjects(int id)
        {
            var level = new Level(id);
            var dict = level.m_LevelObjects;
            LevelObject[] arr = new LevelObject[dict.Count];    
            dict.Values.CopyTo(arr, 0);
            
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = arr[i].Copy();
            }
            
            return arr;
        }
    }
}