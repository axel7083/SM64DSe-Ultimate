namespace SM64DSe.core.Api
{
    public class RomMetadata
    {
        public readonly bool m_IsROMFolder;
        public readonly string m_ROMBasePath;
        public readonly string m_ROMPatchPath;
        public readonly string m_ROMConversionPath;
        public readonly string m_ROMBuildPath;
        public readonly string m_ROMPath;
        
        public RomMetadata(bool mIsRomFolder, string mRomBasePath, string mRomPatchPath, string mRomConversionPath, string mRomBuildPath, string mROMPath)
        {
            m_IsROMFolder = mIsRomFolder;
            m_ROMBasePath = mRomBasePath;
            m_ROMPatchPath = mRomPatchPath;
            m_ROMConversionPath = mRomConversionPath;
            m_ROMBuildPath = mRomBuildPath;
            m_ROMPath = mROMPath;
        }
    }
}