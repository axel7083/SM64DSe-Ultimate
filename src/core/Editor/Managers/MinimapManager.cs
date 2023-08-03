using System;
using System.Drawing;
using System.IO;
using Serilog;
using SM64DSe.ImportExport;

namespace SM64DSe.core.Api
{
    public class MinimapManager : Manager
    {
        public MinimapManager(NitroROM m_ROM) : base(m_ROM)
        {
            
        }
        
        /*
         * Adding caching system for removing all logics from MinimapEditor.cs and let the manager do all the works
         */
        public Bitmap getMinimapByLevelId(ushort levelId, ushort m_CurArea)
        {
            Level level = new Level(levelId);

            if (level.m_MinimapFileIDs == null)
            {
                m_ROM.EndRW();
                return null;
            }
            
            // Loading the files
            NitroFile[] m_TileMapFiles = new NitroFile[level.m_NumAreas];
            
            NitroFile m_PalFile = Program.m_ROM.GetFileFromInternalID(level.m_LevelSettings.MinimapPalFileInternalID);
            NitroFile m_TileSetFile = Program.m_ROM.GetFileFromInternalID(level.m_LevelSettings.MinimapTsetFileInternalID);
            for (int j = 0; j < level.m_NumAreas; j++)
            {
                try
                {
                    if (j < level.m_MinimapFileIDs.Length && level.m_MinimapFileIDs[j] != 0)
                        m_TileMapFiles[j] = (Program.m_ROM.GetFileFromInternalID(level.m_MinimapFileIDs[j]));
                }
                catch (Exception e)
                {
                    Log.Warning($"Something went wrong while getting minimap file for area {j} in level {levelId}. Details: {e.Message}");
                }
            }

            NitroFile m_TileMapFile = m_TileMapFiles[m_CurArea];
            m_TileMapFile.ForceDecompression();// Only to get accurate size below
            int m_SizeX, m_SizeY;
            m_SizeX = m_SizeY = (int)(Math.Sqrt(m_TileMapFile.m_Data.Length / 2) * 8);// Minimaps are squares
            int m_BPP = 8;
            int m_PaletteRow = 0; 
            
            
            string txtSelNCG = m_TileSetFile.m_Name;
            string txtSelNCL = m_PalFile.m_Name;
            string txtSelNSC = m_TileMapFile.m_Name;
            
            // Creating the bitmap
            m_TileSetFile = Program.m_ROM.GetFileFromName(txtSelNCG);
            m_TileSetFile.ForceDecompression();

            m_PalFile = Program.m_ROM.GetFileFromName(txtSelNCL);
            
            bool usingTMap = true;
            m_TileMapFile = Program.m_ROM.GetFileFromName(txtSelNSC);
            m_TileMapFile.ForceDecompression();
            
            Bitmap bmp = new Bitmap(m_SizeX, m_SizeY);

            uint tileoffset = 0, tilenum = 0;
            ushort tilecrap = 0;
            for (int my = 0; my < m_SizeY; my += 8)
            {
                for (int mx = 0; mx < m_SizeY; mx += 8)
                {
                    if (usingTMap)
                    {
                        tilecrap = m_TileMapFile.Read16(tileoffset);
                        tilenum = (uint)(tilecrap & 0x03FF);
                    }

                    for (int ty = 0; ty < 8; ty++)
                    {
                        for (int tx = 0; tx < 8; tx++)
                        {
                            if (m_BPP == 8)
                            {
                                uint totaloffset = (uint)(tilenum * 64 + ty * 8 + tx);//Address of current pixel
                                byte palentry = m_TileSetFile.Read8(totaloffset);//Offset of current pixel's entry in palette file
                                //Palentry is double to get the position of the colour in the palette file
                                ushort pixel = m_PalFile.Read16((uint)(palentry * 2));//Colour of current pixel from palette file
                                bmp.SetPixel(mx + tx, my + ty, Helper.BGR15ToColor(pixel));
                            }
                            else if (m_BPP == 4)
                            {
                                float totaloffset = (float)((float)(tilenum * 64 + ty * 8 + tx) / 2f);//Address of current pixel
                                byte palentry = 0;
                                if (totaloffset % 1 == 0)
                                {
                                    palentry = m_TileSetFile.Read8((uint)totaloffset);//Offset of current pixel's entry in palette file
                                    palentry = (byte)(palentry & 0x0F);// Get 4 right bits
                                }
                                else
                                {
                                    palentry = m_TileSetFile.Read8((uint)totaloffset);//Offset of current pixel's entry in palette file
                                    palentry = (byte)(palentry >> 4);// Get 4 left bits
                                }
                                //Palentry is double to get the position of the colour in the palette file
                                ushort pixel = m_PalFile.Read16((uint)((palentry * 2) + (m_PaletteRow * 32)));//Colour of current pixel from palette file
                                bmp.SetPixel(mx + tx, my + ty, Helper.BGR15ToColor(pixel));
                            }
                        }
                    }

                    tileoffset += 2;
                    if (!usingTMap)
                        tilenum++;
                }
            }
            
            m_ROM.EndRW();
            return bmp;
        }
    }
}