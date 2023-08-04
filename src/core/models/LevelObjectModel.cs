using Newtonsoft.Json;
using OpenTK;

namespace SM64DSe.core.models
{
    public class LevelObjectModel
    {
        [JsonProperty]
        public ushort ID;
        
        [JsonProperty]
        public Vector3 Position;
        
        [JsonProperty]
        public float YRotation;
        
        // object specific parameters
        // for standard objects: [0] = 16bit object param, [1] and [2] = what should be X and Z rotation
        // for simple objects: [0] = 7bit object param
        [JsonProperty]
        public ushort[] Parameters;
        
        [JsonProperty]
        public int m_Layer;
        [JsonProperty]
        public int m_Area;
        [JsonProperty]
        public uint m_UniqueID;
        
        [JsonProperty]
        public LevelObject.Type m_Type;
        
        [JsonIgnore]
        public ObjectRenderer m_Renderer;
        [JsonIgnore]
        public PropertyTable m_Properties;
        
        public ParameterField[] m_ParameterFields = null;
    }
}