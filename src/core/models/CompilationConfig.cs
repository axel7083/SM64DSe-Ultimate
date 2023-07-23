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
        
        public OverlayTarget(string path, int overlayId) : base(path)
        {
            OverlayId = overlayId;
        }
        
        public OverlayTarget(string path, int overlayId, int ramAddress) : base(path)
        {
            OverlayId = overlayId;
            RamAddress = ramAddress;
        }
    }
    
    [JsonObject(MemberSerialization.OptIn)]
    public class CompilationConfig
    {
        [JsonProperty]
        public List<OverlayTarget> overlays { get; set; }

        /*
         Not implemented yet
        [JsonProperty]
        public List<DlTarget> dls { get; set; }
        */

        [JsonProperty]
        public CompilationTarget patch { get; set; }

        public CompilationConfig()
        {
            overlays = new List<OverlayTarget>();
        }
    }
}