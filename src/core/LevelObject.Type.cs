namespace SM64DSe
{
    public partial class LevelObject
    {
        public enum Type
        {
            STANDARD = 0,
            ENTRANCE = 1, 
            PATH_NODE = 2, 
            PATH = 3, 
            VIEW = 4, 
            SIMPLE = 5, 
            TELEPORT_SOURCE = 6, 
            TELEPORT_DESTINATION = 7, 
            FOG = 8, 
            DOOR = 9, 
            EXIT = 10, 
            MINIMAP_TILE_ID = 11, 
            MINIMAP_SCALE = 12, 
            UNKNOWN = 13, // to avoid having issues when converting enum to array. Should not be used
            STAR_CAMERAS = 14
        };
    }
}