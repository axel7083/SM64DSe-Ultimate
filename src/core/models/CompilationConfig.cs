using System.Collections.Generic;
using Newtonsoft.Json;

namespace SM64DSe.core.models
{
    public enum CompilationStatus
    {
        PENDING = 0,
        FAILED = 1,
        SUCCESS = 2,
    }

    public class SM64DSObject
    {
        [JsonProperty]
        public int id;
        
        [JsonProperty]
        public int category;
        
        [JsonProperty]
        public string name;
        
        [JsonProperty]
        public string internalname;
        
        [JsonProperty]
        public string actorid;
        
        [JsonProperty]
        public string description;
        
        [JsonProperty]
        public string icon = null;
        
        public SM64DSObject() { }
    }
    
    public class CompilationTarget
    {
        [JsonProperty]
        public string Path { get; set; }

        public CompilationStatus Status { get; set; } = CompilationStatus.PENDING;

        public CompilationTarget(string path)
        {
            Path = path;
        }
        
        public CompilationTarget() {}
    }

    public class OverlayTarget : CompilationTarget
    {
        [JsonProperty] public int OverlayId { get; set; }

        [JsonProperty] public int RamAddress { get; set; } = -1;
        
        [JsonProperty] public SM64DSObject[] Objects { get; set; }
        
        public OverlayTarget(string path, int overlayId, SM64DSObject[] objects = null) : base(path)
        {
            OverlayId = overlayId;
            Objects = objects;
        }
        
        public OverlayTarget(string path, int overlayId, int ramAddress, SM64DSObject[] objects = null) : base(path)
        {
            OverlayId = overlayId;
            RamAddress = ramAddress;
            Objects = objects;
        }

        public OverlayTarget() { }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class CompilationConfig
    {
        [JsonProperty]
        public List<OverlayTarget> Overlays { get; set; }

        /*
         Not implemented yet
        [JsonProperty]
        public List<DlTarget> dls { get; set; }
        */

        [JsonProperty]
        public CompilationTarget Patch { get; set; }

        public CompilationConfig()
        {
            Overlays = new List<OverlayTarget>();
        }
    }
}