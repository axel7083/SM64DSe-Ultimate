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
    }
    
    
}