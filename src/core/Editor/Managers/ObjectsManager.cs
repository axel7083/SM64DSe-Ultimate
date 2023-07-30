using System;
using System.IO;
using SM64DSe.ImportExport;

namespace SM64DSe.core.Api
{
    public class ObjectsManager : Manager
    {
        public ObjectsManager(NitroROM m_ROM) : base(m_ROM)
        {
            
        }

        public ObjectRenderer getObjectRendererByLevelObject(LevelObject obj)
        {
            return ObjectRenderer.FromLevelObject(obj);
        }
    }
}