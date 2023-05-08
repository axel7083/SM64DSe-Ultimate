using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace SM64DSe.sources.editors
{
    public static class TextUtils
    {
        public static BiDictionaryOneToOne<byte, string> BASIC_EUR_US_CHARS = new BiDictionaryOneToOne<byte,string>();
        public static BiDictionaryOneToOne<byte, string> EXTENDED_ASCII_CHARS = new BiDictionaryOneToOne<byte,string>();
        public static BiDictionaryOneToOne<byte, string> JAP_CHARS = new BiDictionaryOneToOne<byte, string>();

        public static Dictionary<string, uint> BASIC_EUR_US_SIZES = new Dictionary<string, uint>();
        public static Dictionary<string, uint> EXTENDED_ASCII_SIZES = new Dictionary<string, uint>();
        public static Dictionary<string, uint> JAP_SIZES = new Dictionary<string, uint>();

        public static void InitDictionaries()
        {
            LoadCharList("extended_ascii.txt", EXTENDED_ASCII_CHARS, EXTENDED_ASCII_SIZES);
            LoadCharList("basic_eur_us_chars.txt", BASIC_EUR_US_CHARS, BASIC_EUR_US_SIZES);
            LoadCharList("jap_chars.txt", JAP_CHARS, JAP_SIZES);
        }
        
        public static void LoadCharList(string txtName, BiDictionaryOneToOne<byte, string> charList,
            Dictionary<string, uint> sizeList)
        {
            string filename = Path.Combine(Application.StartupPath, txtName);
            string text = File.ReadAllText(filename);

            string[] lines = text.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                // Ignore comments
                if (lines[i].ToCharArray()[0] == '#')
                    continue;

                string[] pair = lines[i].Split('=');
                if (pair.Length < 3)
                    continue;

                try { 
                    charList.Add(byte.Parse(pair[0]), pair[2]); 
                    sizeList.Add(pair[2], uint.Parse(pair[1]));
                }
                catch (Exception e) { MessageBox.Show("Error in " + filename + "\n\n" + "Line " + i + "\n\n" + 
                                                      pair[0] + "\t" + pair[1] + "\t" + pair[2] + "\n\n" + e.Message); }
            }

        }
        
        
        public static string ShortVersion(string msg, int index, int limit = 45)
        {
            string shortversion = msg.Replace("\r\n", " "); // TODO: investigate ??? shortversion is replaced without being used.
            shortversion = (msg.Length > limit) ? msg.Substring(0, limit - 3) + "..." : msg;
            shortversion = string.Format("[{0:X4}] {1}", index, shortversion);
            return shortversion;
        }
        
        public static List<byte> EncodeString(string msg, nitro.NitroROM.Version version)
        {
            var newMsg = msg.Replace("[\\r]", "\r");
            char[] newTextByte = newMsg.ToCharArray();
            List<byte> encodedString = new List<byte>();

            int i = 0;
            while (i < newTextByte.Length)
            {
                /*
                // Upper
                // nintendo encoding = ('A' + cur - 0x0A);
                // ascii = A + ne - 0x0A
                // ascii - A + 0x0A = ne
                if (Char.IsNumber(newTextByte[i]))// Numeric
                    encodedString.Add((byte)(newTextByte[i] - '0'));
                else if (newTextByte[i] >= 0x41 && newTextByte[i] <= 0x5A)//Uppercase
                    encodedString.Add((byte)(newTextByte[i] - 'A' + 0x0A));
                else if (newTextByte[i] >= 0x61 && newTextByte[i] <= 0x7A)// Lowercase
                    encodedString.Add((byte)(newTextByte[i] - 'a' + 0x2D));
                else if (newTextByte[i] >= 0x80 && newTextByte[i] < (0xFF + 0x01))// Extended characters 128 to 255
                    encodedString.Add((byte)(newTextByte[i] - 0x30));// Character - offset of 0x30 to get Nintendo character*/

                if (version == nitro.NitroROM.Version.JAP)
                {
                    if (JAP_CHARS.GetSecondToFirst().ContainsKey("" + newTextByte[i]))
                        encodedString.Add(JAP_CHARS.GetBySecond("" + newTextByte[i]));
                }
                else
                {
                    if (BASIC_EUR_US_CHARS.GetSecondToFirst().ContainsKey("" + newTextByte[i]))
                        encodedString.Add(BASIC_EUR_US_CHARS.GetBySecond("" + newTextByte[i]));
                    else if (EXTENDED_ASCII_CHARS.GetSecondToFirst().ContainsKey("" + newTextByte[i]))
                        encodedString.Add(EXTENDED_ASCII_CHARS.GetBySecond("" + newTextByte[i]));
                }
                if (newTextByte[i].Equals('\r'))// New Line is \r\n
                {
                    i++;// Point after r
                    if (newTextByte[i].Equals('\n'))
                    {
                        encodedString.Add((byte)0xFD);
                        i++;
                        continue;
                    }
                    // 0xFE denotes special character
                    else if (newTextByte[i].Equals('F') && newTextByte[i + 1].Equals('E'))
                    {
                        //FE 05 03 00 06 - [R) glyph
                        //FE 07 01 00 00 00 XX - number of stars till you get XX
                        String byte2 = "" + newTextByte[i + 2] + newTextByte[i + 3];
                        int len = int.Parse(byte2, System.Globalization.NumberStyles.HexNumber);
                        for (int j = 0; j < (len * 2); j += 2)
                        {
                            String temp = "" + newTextByte[i + j] + newTextByte[i + j + 1];
                            encodedString.Add((byte)int.Parse(temp, System.Globalization.NumberStyles.HexNumber));
                        }
                        i += (len * 2);

                        continue;
                    }
                    else
                    {
                        // Special characters [\r]C [\r]S [\r]s [\r]D [\r]A [\r]B [\r]X [\r]Y

                        string specialChar = "[\\r]" + newTextByte[i];
                        uint size = 0;
                        byte val = 0xFF;

                        if (version == nitro.NitroROM.Version.JAP)
                        {
                            size = JAP_SIZES[specialChar];
                            val = JAP_CHARS.GetBySecond(specialChar);
                        }
                        else
                        {
                            if (BASIC_EUR_US_SIZES.ContainsKey(specialChar))
                                size = BASIC_EUR_US_SIZES[specialChar];
                            else if (EXTENDED_ASCII_SIZES.ContainsKey(specialChar))
                                size = EXTENDED_ASCII_SIZES[specialChar];

                            if (BASIC_EUR_US_CHARS.GetSecondToFirst().ContainsKey(specialChar))
                                val = BASIC_EUR_US_CHARS.GetBySecond(specialChar);
                            else if (EXTENDED_ASCII_CHARS.GetSecondToFirst().ContainsKey(specialChar))
                                val = EXTENDED_ASCII_CHARS.GetBySecond(specialChar);
                        }

                        for (int j = 0; j < size; j++)
                        {
                            encodedString.Add((byte)(val + j));
                        }

                        i++;
                        continue;
                    }
                }
                i++;
            }

            encodedString.Add(0xFF);// End of message

            return encodedString;
        }
    }
    
    
}