using Newtonsoft.Json;

namespace SM64DSe.core.models
{
    public class LevelObjects
    {
        [JsonProperty]
        LevelObject[] objects;

        public LevelObjects(LevelObject[] objects)
        {
            this.objects = objects;
        }
    }
}