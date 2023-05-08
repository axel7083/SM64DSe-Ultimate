using System;
using System.Collections.Generic;
using System.IO;

namespace SM64DSe.sources.editors
{
    public class TextEditor
    {
        private NitroROM rom;
        
        private static BiDictionaryOneToOne<byte, string> BASIC_EUR_US_CHARS = new BiDictionaryOneToOne<byte,string>();
        private static BiDictionaryOneToOne<byte, string> EXTENDED_ASCII_CHARS = new BiDictionaryOneToOne<byte,string>();
        private static BiDictionaryOneToOne<byte, string> JAP_CHARS = new BiDictionaryOneToOne<byte, string>();

        private static Dictionary<string, uint> BASIC_EUR_US_SIZES = new Dictionary<string, uint>();
        private static Dictionary<string, uint> EXTENDED_ASCII_SIZES = new Dictionary<string, uint>();
        private static Dictionary<string, uint> JAP_SIZES = new Dictionary<string, uint>();
        
        public TextEditor(NitroROM rom)
        {
            this.rom = rom;
            TextUtils.LoadCharList("extended_ascii.txt", EXTENDED_ASCII_CHARS, EXTENDED_ASCII_SIZES);
            TextUtils.LoadCharList("basic_eur_us_chars.txt", BASIC_EUR_US_CHARS, BASIC_EUR_US_SIZES);
            TextUtils.LoadCharList("jap_chars.txt", JAP_CHARS, JAP_SIZES);
        }

        private NitroROM.Version GetRomVersion()
        {
            return rom.m_Version;
        }

        public KeyValuePair<string, string>[] GetLanguages()
        {
            switch (GetRomVersion())
            {
                case NitroROM.Version.USA_v1:
                case NitroROM.Version.USA_v2:
                case NitroROM.Version.JAP:
                    return new[]
                    {
                        new KeyValuePair<string, string>("English", "nes"), 
                        new KeyValuePair<string, string>("Japanese", "jpn")
                    };
                case NitroROM.Version.EUR:
                    return new[]
                    {
                        new KeyValuePair<string, string>("English", "eng"), 
                        new KeyValuePair<string, string>("Français", "frc"),
                        new KeyValuePair<string, string>("Deutsch", "gmn"),
                        new KeyValuePair<string, string>("Italiano", "itl"),
                        new KeyValuePair<string, string>("Español", "spn"),
                    };
                default:
                    throw new NotSupportedException("Unsupported ROM version");
            }
        }
        
        private bool copyMessage;
        private string[] m_MsgData;
        private int[] m_StringLengths;
        private string[] m_ShortVersions;
        private NitroFile file;
        private uint inf1size;
        private uint m_FileSize;
        private uint m_DAT1Start;// Address at which the string data is held
        private uint[] m_StringHeaderAddr;// The addresses of the string headers
        private uint[] m_StringHeaderData;// The offsets of the strings (relative to start of DAT1 section)
        private uint[] m_StringWidthAddr;
        private ushort[] m_StringWidth;
        private uint[] m_StringHeightAddr;
        private ushort[] m_StringHeight;
        private List<int> m_EditedEntries = new List<int>();// Holds indices of edited entries, needed because of how old and new strings are stored differently
        
        public void LoadLanguagesFile(string key)
        {
            var filename = "data/message/msg_data_" + key + ".bin";
            
            // Ensure the file exist
            if (rom.GetFileIDFromName(filename) == 0xFFFF)
                throw new FileNotFoundException("The file requested does not exist.", filename);

            file = rom.GetFileFromName(filename);
            
            inf1size = file.Read32(0x24);
            var numentries = file.Read16(0x28);
            
            m_MsgData = new string[numentries];
            m_StringLengths = new int[numentries];
            m_ShortVersions = new string[numentries];
            m_FileSize = file.Read32(0x08);
            m_StringHeaderAddr = new uint[numentries];
            m_StringHeaderData = new uint[numentries];
            m_StringWidthAddr = new uint[numentries];
            m_StringWidth = new ushort[numentries];
            m_StringHeightAddr = new uint[numentries];
            m_StringHeight = new ushort[numentries];
            m_DAT1Start = 0x20 + inf1size + 0x08;
            
            for (var i = 0; i < numentries; i++)
            {
                m_StringHeaderAddr[i] = (uint)(0x20 + 0x10 + (i * 8));
                m_StringHeaderData[i] = file.Read32(m_StringHeaderAddr[i]);
                m_StringWidthAddr[i] = (uint)(0x20 + 0x10 + (i * 8) + 0x4);
                m_StringWidth[i] = file.Read16(m_StringWidthAddr[i]);
                m_StringHeightAddr[i] = (uint)(0x20 + 0x10 + (i * 8) + 0x6);
                m_StringHeight[i] = file.Read16(m_StringHeightAddr[i]);
            }
            
            for (var i = 0; i < m_MsgData.Length; i++)
            {
                uint straddr = file.Read32((uint)(0x30 + i * 8));
                straddr += 0x20 + inf1size + 0x8;

                int length = 0;

                string thetext = "";
                for (; ; )
                {
                    byte cur;
                    try
                    {
                        cur = file.Read8(straddr);
                    }
                    catch
                    {
                        break;
                    }
                    straddr++;
                    length++;
                    char thechar = '\0';

                    /*if ((cur >= 0x00) && (cur <= 0x09))
                        thechar = (char)('0' + cur);
                    else if ((cur >= 0x0A) && (cur <= 0x23))
                        thechar = (char)('A' + cur - 0x0A);
                    else if ((cur >= 0x2D) && (cur <= 0x46))
                        thechar = (char)('a' + cur - 0x2D);
                    else if ((cur >= 0x50) && (cur <= 0xCF))//Extended ASCII Characters
                        thechar = (char)(0x30 + cur);*/
                    // Some characters are two bytes long, can skip the second

                    if (GetRomVersion() == NitroROM.Version.JAP)
                    {
                        if (JAP_CHARS.GetFirstToSecond().ContainsKey(cur))
                        {
                            thetext += JAP_CHARS.GetByFirst(cur);
                            straddr += (JAP_SIZES[JAP_CHARS.GetByFirst(cur)] - 1);
                            length += (int)(JAP_SIZES[JAP_CHARS.GetByFirst(cur)] - 1);
                        }
                    }
                    else
                    {
                        if ((cur >= 0x00 && cur <= 0x4F) || (cur >= 0xEE && cur <= 0xFB))
                        {
                            thetext += BASIC_EUR_US_CHARS.GetByFirst(cur);
                            straddr += (BASIC_EUR_US_SIZES[BASIC_EUR_US_CHARS.GetByFirst(cur)] - 1);
                            length += (int)(BASIC_EUR_US_SIZES[BASIC_EUR_US_CHARS.GetByFirst(cur)] - 1);
                        }
                        else if (cur >= 0x50 && cur <= 0xCF)
                        {
                            thetext += EXTENDED_ASCII_CHARS.GetByFirst(cur);
                            straddr += (EXTENDED_ASCII_SIZES[EXTENDED_ASCII_CHARS.GetByFirst(cur)] - 1);
                            length += (int)(EXTENDED_ASCII_SIZES[EXTENDED_ASCII_CHARS.GetByFirst(cur)] - 1);
                        }
                    }

                    if (thechar != '\0')
                        thetext += thechar;
                    else if (cur == 0xFD)
                        thetext += "\r\n";
                    else if (cur == 0xFF)
                        break;
                    else if (cur == 0xFE)// Special Character
                    {
                        int len = file.Read8(straddr);
                        thetext += "[\\r]";
                        thetext += String.Format("{0:X2}", cur);
                        for (int spec = 0; spec < len - 1; spec++)
                        {
                            thetext += String.Format("{0:X2}", file.Read8((uint)(straddr + spec)));
                        }
                        length += (len - 1);// Already increased by 1 at start
                        straddr += (uint)(len - 1);
                    }
                }

                m_MsgData[i] = thetext;
                m_StringLengths[i] = length;
                m_ShortVersions[i] = TextUtils.ShortVersion(m_MsgData[i], i);
            }
        }
    }
}