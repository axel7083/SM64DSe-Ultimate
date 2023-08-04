using Newtonsoft.Json;

namespace SM64DSe.core.models
{
    public class LevelObjects
    {
        [JsonProperty]
        public readonly LevelObjectModel[] objects;

        public LevelObjects(LevelObjectModel[] objects)
        {
            this.objects = objects;
        }
    }
}