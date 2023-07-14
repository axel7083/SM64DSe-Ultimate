using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SM64DSe.sources.editors
{
    public struct MessagesDetails
    {
        public string message;
        public string shortMessage;
        public int width;
        public int height;
        public MessagesDetails(string message, string shortMessage, int width, int height)
        {
            this.message = message;
            this.shortMessage = shortMessage;
            this.width = width;
            this.height = height;
        }
    }
    
    public class TextEditor
    {
        private nitro.NitroROM rom;
        private int LIMIT = 45;
        
        public TextEditor(nitro.NitroROM rom)
        {
            this.rom = rom;
            TextUtils.InitDictionaries();
        }

        private nitro.NitroROM.Version GetRomVersion()
        {
            return rom.m_Version;
        }

        public Dictionary<string, string> GetLanguages()
        {
            switch (GetRomVersion())
            {
                case nitro.NitroROM.Version.USA_v1:
                case nitro.NitroROM.Version.USA_v2:
                case nitro.NitroROM.Version.JAP:
                    return new Dictionary<string, string>()
                    {
                        {"English", "nes"},
                        {"Japanese", "jpn"},
                    };
                case nitro.NitroROM.Version.EUR:
                    return new Dictionary<string, string>()
                    {
                        {"English", "eng"}, 
                        {"Français", "frn"},
                        {"Deutsch", "gmn"},
                        {"Italiano", "itl"},
                        {"Español", "spn"},
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


        private string currentLanguage = null;
        
        public void LoadEnglish()
        {
            LoadLanguagesFile(GetLanguages()["English"]);
        }
        
        public void LoadLanguagesFile(string key)
        {
            // If we already loaded the current language and no modification has been made.
            if (currentLanguage == key && m_EditedEntries.Count == 0)
                return;

            currentLanguage = key;
            
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

                    if (GetRomVersion() == nitro.NitroROM.Version.JAP)
                    {
                        if (TextUtils.JAP_CHARS.GetFirstToSecond().ContainsKey(cur))
                        {
                            thetext += TextUtils.JAP_CHARS.GetByFirst(cur);
                            straddr += (TextUtils.JAP_SIZES[TextUtils.JAP_CHARS.GetByFirst(cur)] - 1);
                            length += (int)(TextUtils.JAP_SIZES[TextUtils.JAP_CHARS.GetByFirst(cur)] - 1);
                        }
                    }
                    else
                    {
                        if ((cur >= 0x00 && cur <= 0x4F) || (cur >= 0xEE && cur <= 0xFB))
                        {
                            thetext += TextUtils.BASIC_EUR_US_CHARS.GetByFirst(cur);
                            straddr += (TextUtils.BASIC_EUR_US_SIZES[TextUtils.BASIC_EUR_US_CHARS.GetByFirst(cur)] - 1);
                            length += (int)(TextUtils.BASIC_EUR_US_SIZES[TextUtils.BASIC_EUR_US_CHARS.GetByFirst(cur)] - 1);
                        }
                        else if (cur >= 0x50 && cur <= 0xCF)
                        {
                            thetext += TextUtils.EXTENDED_ASCII_CHARS.GetByFirst(cur);
                            straddr += (TextUtils.EXTENDED_ASCII_SIZES[TextUtils.EXTENDED_ASCII_CHARS.GetByFirst(cur)] - 1);
                            length += (int)(TextUtils.EXTENDED_ASCII_SIZES[TextUtils.EXTENDED_ASCII_CHARS.GetByFirst(cur)] - 1);
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
        
        ushort[] MSG_ID_CHAR_MAP;

        private ushort GetMessageIdByParameterValue(ushort id)
        {
            if (MSG_ID_CHAR_MAP == null || MSG_ID_CHAR_MAP.Length == 0)
            {
                rom.BeginRW();
                
                // Read the block of bytes from the given address
                byte[] block = rom.ReadBlock(0x0008EEEC, 196 * 2);

                // Convert the bytes into an array of shorts
                MSG_ID_CHAR_MAP = new ushort[196];
                for (var i = 0; i < 196; i++)
                {
                    MSG_ID_CHAR_MAP[i] = (ushort)(block[2*i] | (block[2*i+1] << 8));
                }
            
                rom.EndRW();
            }

            // The structure of the mapping is an array of 96 ushort 
            for (int i = 0; i < 98; i+=2)
            {
                if (MSG_ID_CHAR_MAP[i] == id)
                    return MSG_ID_CHAR_MAP[i + 1];
            }

            throw new ArgumentOutOfRangeException("The provided parameter could not be mapped with a known text.");
        }

        public string[] GetAllMessages()
        {
            if (m_MsgData == null || m_MsgData.Length == 0)
                throw new Exception("The messages have not been loaded. You need to select a language first.");
            
            return m_MsgData;
        }
        
        public string[] GetAllShortMessages()
        {
            if (m_ShortVersions == null || m_ShortVersions.Length == 0)
                throw new Exception("The messages have not been loaded. You need to select a language first.");
            
            return m_ShortVersions;
        }

        public MessagesDetails getMessageDetails(int index)
        {
            if (index >= m_MsgData.Length)
                throw new IndexOutOfRangeException("The Message index is out of range.");

            return new MessagesDetails(m_MsgData[index], m_ShortVersions[index], m_StringWidth[index], m_StringHeight[index]);
        }

        public bool isValidParameterValue(ushort parameter)
        {
            try
            {
                GetMessageIdByParameterValue(parameter);
                return true;
            } 
            catch (ArgumentOutOfRangeException e)
            {
                return false;
            }
        }

        public string getMessageByParameterValue(ushort parameter)
        {
            var index = GetMessageIdByParameterValue(parameter);
            if (index >= m_MsgData.Length)
                throw new IndexOutOfRangeException("The Message index is out of range.");

            return GetAllMessages()[index];
        }

        public string getShortMessageByParameterValue(ushort parameter)
        {
            var index = GetMessageIdByParameterValue(parameter);
            if (index >= m_ShortVersions.Length)
                throw new IndexOutOfRangeException("The Message index is out of range.");

            if (m_ShortVersions == null || m_ShortVersions.Length == 0)
                throw new Exception("The messages have not been loaded. You need to select a language first.");
            
            return m_ShortVersions[index];
        }
        
        public void UpdateEntries(String msg, int index)
        {
            m_MsgData[index] = msg;
            m_ShortVersions[index] = TextUtils.ShortVersion(msg, index);
            int lengthDif = TextUtils.EncodeString(msg, GetRomVersion()).Count - m_StringLengths[index];
            m_StringLengths[index] += lengthDif;

            //Make or remove room for the new string if needed (don't need to for last entry)
            if (lengthDif > 0 && index != m_MsgData.Length - 1)
            {
                uint curStringStart = m_StringHeaderData[index] + m_DAT1Start;
                uint nextStringStart = m_StringHeaderData[index + 1] + m_DAT1Start;
                byte[] followingData = file.ReadBlock(nextStringStart, (uint)(file.m_Data.Length - nextStringStart));
                for (int i = (int)curStringStart; i < (int)nextStringStart + lengthDif; i++)
                {
                    file.Write8((uint)i, 0);// Fill the gap with zeroes
                }
                file.WriteBlock((uint)(nextStringStart + lengthDif), followingData);
            }
            else if (lengthDif < 0 && index != m_MsgData.Length - 1)
            {
                // lengthDif is negative, -- +
                uint nextStringStart = m_StringHeaderData[index + 1] + m_DAT1Start;
                byte[] followingData = file.ReadBlock(nextStringStart, (uint)(file.m_Data.Length - nextStringStart));
                file.WriteBlock((uint)(nextStringStart + lengthDif), followingData);
                int oldSize = file.m_Data.Length;
                Array.Resize(ref file.m_Data, oldSize + lengthDif);// Remove duplicate data at end of file
            }

            // Update pointers to string entry data
            if (lengthDif != 0)
            {
                for (int i = index + 1; i < m_MsgData.Length; i++)
                {
                    if (lengthDif > 0)
                        m_StringHeaderData[i] += (uint)lengthDif;
                    else if (lengthDif < 0)
                        m_StringHeaderData[i] = (uint)(m_StringHeaderData[i] + lengthDif);

                    file.Write32(m_StringHeaderAddr[i], m_StringHeaderData[i]);
                    file.Write16(m_StringWidthAddr[i], m_StringWidth[i]);
                    file.Write16(m_StringHeightAddr[i], m_StringHeight[i]);
                }
            }
            // Update total file size
            file.Write32(0x08, (uint)(int)(file.Read32(0x08) + lengthDif));
            // Update DAT1 size
            file.Write32(m_DAT1Start - 0x04, (uint)(int)(file.Read32(m_DAT1Start - 0x04) + lengthDif));

            // Keep track of the modified entries
            m_EditedEntries.Add(index);
        }

        public void updateWidth(ushort value, int index)
        {
            m_StringWidth[index] = value;
        }
        
        public void updateHeight(ushort value, int index)
        {
            m_StringHeight[index] = value;
        }
        
        
        public void WriteData()
        {
            // Encode and write all edited string entries
            foreach (int index in m_EditedEntries)
            {
                List<byte> entry = TextUtils.EncodeString(m_MsgData[index], GetRomVersion());
                file.WriteBlock(m_StringHeaderData[index] + m_DAT1Start, entry.ToArray());
            }

            for (int i = 0; i < file.Read16(0x28); i++)
            {
                file.Write32(m_StringHeaderAddr[i], m_StringHeaderData[i]);
                file.Write16(m_StringWidthAddr[i], m_StringWidth[i]);
                file.Write16(m_StringHeightAddr[i], m_StringHeight[i]);
            }

            // Save changes
            file.SaveChanges();
            
            // Clean entries
            m_EditedEntries.Clear();
        }
        
        public void ImportXML(XmlReader reader)
        {
            reader.MoveToContent();

            int i = 0;
            while (reader.Read())
            {
                if (reader.NodeType.Equals(XmlNodeType.Element))
                {
                    switch (reader.LocalName)
                    {
                        case "Text":
                            if (i < m_MsgData.Length)
                            {
                                String temp = reader.ReadElementContentAsString();
                                temp = temp.Replace("\n", "\r\n");
                                temp = temp.Replace("[\\r]", "\r");
                                m_MsgData[i] = temp;
                            }
                            i++;
                            break;
                    }
                }
            }

            for (int j = 0; j < m_MsgData.Length; j++)
            {
                UpdateEntries(m_MsgData[j], j);
                List<byte> entry = TextUtils.EncodeString(m_MsgData[j], GetRomVersion());
                file.WriteBlock(m_StringHeaderData[j] + m_DAT1Start, entry.ToArray());
            }

            file.SaveChanges();
        }
        
        public void ExportXML(XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteComment(Program.AppTitle + " " + Program.AppVersion + " " + Program.AppDate);
            writer.WriteStartElement("SM64DS_Texts");
                
            for (int i = 0; i < m_MsgData.Length; i++)
            {
                writer.WriteStartElement("Text");
                writer.WriteAttributeString("index", i.ToString());
                writer.WriteAttributeString("id", String.Format("{0:X4}", i));
                writer.WriteString(m_MsgData[i]);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

    }
}