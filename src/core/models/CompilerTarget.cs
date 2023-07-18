namespace SM64DSe.core.models
{
    public enum CompilationStatus
    {
        PENDING = 0,
        FAILED = 1,
        SUCCESS = 2,
    }
    
    
    public class CompilerTarget
    {
        public uint OverlayId { get; }

        public string Path { get; }

        public CompilationStatus Status { get; set; }

        public CompilerTarget(uint overlayId, string path, CompilationStatus status)
        {
            this.OverlayId = overlayId;
            this.Path = path;
            this.Status = status;
        }
        
        
    }
}