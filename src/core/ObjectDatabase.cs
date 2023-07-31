/*
    Copyright 2012 Kuribo64

    This file is part of SM64DSe.

    SM64DSe is free software: you can redistribute it and/or modify it under
    the terms of the GNU General Public License as published by the Free
    Software Foundation, either version 3 of the License, or (at your option)
    any later version.

    SM64DSe is distributed in the hope that it will be useful, but WITHOUT ANY 
    WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
    FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along 
    with SM64DSe. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using OpenTK;

namespace SM64DSe
{
    public static class ObjectDatabase
    {
        public class RendererConfig
        {
            public string type;
            public string[] files;
            public float scale;
            public Vector3[] offsets;
            public string animation;
            public string border;
            public string fill;

            public RendererConfig(string type = null, string[] files = null, float scale = 1f, Vector3[] offsets = null, string animation = null)
            {
                this.type = type;
                this.files = files;
                this.scale = scale;
                this.offsets = offsets;
                this.animation = animation;
            }

            public RendererConfig(string border, string fill, string animation = null) 
            {
                this.border = border;
                this.fill = fill;
            }
        }
        public class ObjectInfo
        {
            public struct ParamInfo
            {
                public string m_Name;
                public int m_Offset, m_Length;
                public string m_Type, m_Values;
                public string m_Description;
            }

            public string GetBasicInfo()
            {
                return (m_Name + "\n" + m_InternalName + "\n" + m_Description);
            }
            
            [JsonProperty]
            public string[] m_dlRequirements;
            
            [JsonProperty]
            public RendererConfig m_Renderer;

            [JsonProperty]
            public ushort m_ID;
            [JsonProperty]
            public ushort m_ActorID;
            [JsonProperty]
            public string m_Name;
            [JsonProperty]
            public string m_InternalName;
            
            public string m_Description;

            public int m_BankRequirement;
            public int m_NumBank, m_BankSetting;

            public string m_IsCustomModelPath;

            public ParamInfo[] m_ParamInfo;
        }

        public static ObjectInfo[] m_ObjectInfo = null;
        public static uint m_Timestamp;
        public static WebClient m_WebClient;


        public static void Initialize()
        {
            m_ObjectInfo = new ObjectInfo[65536];
            for (int i = 0; i < 65536; i++)
                m_ObjectInfo[i] = new ObjectInfo();

            m_WebClient = new WebClient();
        }

        public static void AddObjects(ObjectInfo[] nObjects)
        {
            foreach (var nObject in nObjects)
            {
                m_ObjectInfo[nObject.m_ID] = nObject;
            }
        }

        /**
         * @str: example "9,7,6" 
         */
        [Pure]
        public static Vector3 ParseVector3FromString(string str)
        {
            var arr = str.Split(',');
            if (arr.Length != 3)
                throw new FormatException($"The input {str} is incorrect.");
            return new Vector3(
                float.Parse(arr[0]),
                float.Parse(arr[1]),
                float.Parse(arr[2])
            );
        }
        public static void Load()
        {
            FileStream fs = null; XmlReader xr = null;
            try 
            { 
                fs = File.OpenRead("assets/objectdb.xml"); 
                xr = XmlReader.Create(fs);

                xr.ReadToFollowing("database");
                xr.MoveToAttribute("timestamp");
                m_Timestamp = uint.Parse(xr.Value);
            }
            catch
            {
                if (xr != null) xr.Close();
                if (fs != null) fs.Close();

                m_Timestamp = 1;
                throw new Exception("Failed to open objectdb.xml");
            }

            while (xr.ReadToFollowing("object"))
            {
                string temp;

                xr.MoveToAttribute("id");
                int id = 0; int.TryParse(xr.Value, out id);
                if ((id < 0) || (id > m_ObjectInfo.Length))
                    continue;

                ObjectInfo oinfo = m_ObjectInfo[id];
                oinfo.m_ID = (ushort)id;

                xr.ReadToFollowing("name");
                oinfo.m_Name = xr.ReadElementContentAsString();
                xr.ReadToFollowing("internalname");
                oinfo.m_InternalName = xr.ReadElementContentAsString();
                if (oinfo.m_InternalName.StartsWith("@CUSTOM%")) { oinfo.m_IsCustomModelPath = oinfo.m_InternalName.Substring(8); } else { oinfo.m_IsCustomModelPath = null; }

                if (oinfo.m_Name == "")
                    oinfo.m_Name = oinfo.m_InternalName;

                xr.ReadToFollowing("actorid");
                temp = xr.ReadElementContentAsString();
                ushort.TryParse(temp, out oinfo.m_ActorID);

                xr.ReadToFollowing("description");
                oinfo.m_Description = xr.ReadElementContentAsString();

                xr.ReadToFollowing("bankreq");
                temp = xr.ReadElementContentAsString();
                if (temp == "none")
                    oinfo.m_BankRequirement = 0;
                else
                {
                    oinfo.m_BankRequirement = 1;
                    try
                    {
                        oinfo.m_NumBank = int.Parse(temp.Substring(0, temp.IndexOf('=')));
                        oinfo.m_BankSetting = int.Parse(temp.Substring(temp.IndexOf('=') + 1));
                    }
                    catch { oinfo.m_BankRequirement = 2; }
                }
                
                xr.ReadToFollowing("dlreq");
                temp = xr.ReadElementContentAsString();
                if (temp == "none")
                    oinfo.m_dlRequirements = null;
                else
                    oinfo.m_dlRequirements = temp.Split(' ');
                
                xr.ReadToFollowing("renderer");
                string type = xr.GetAttribute("type");
                string scale = xr.GetAttribute("scale");
                
                switch (type)
                {
                    case "NormalBMD":
                    case "NormalKCL":
                        oinfo.m_Renderer = new RendererConfig(
                            type,
                            new string[] { xr.GetAttribute("file") },
                            scale != null? float.Parse(scale) : 1f
                            );
                        break;
                    case "DoubleBMD":
                        Vector3[] offsets = null;
                        string offset1 = xr.GetAttribute("offset1");
                        if (!String.IsNullOrEmpty(offset1))
                        {
                            offsets = new []
                            {
                                ParseVector3FromString(offset1),
                                ParseVector3FromString(xr.GetAttribute("offset2"))
                            };
                        }
                        
                        oinfo.m_Renderer = new RendererConfig(
                            type,
                            new string[] { xr.GetAttribute("file1"), xr.GetAttribute("file2") },
                            scale != null? float.Parse(scale) : 1f,
                            offsets
                        );
                        break;
                    case "Kurumajiku":
                        oinfo.m_Renderer = new RendererConfig(
                            type: type,
                            files: new string[]
                            {
                                xr.GetAttribute("file1"),
                                xr.GetAttribute("file2")
                            },
                            scale != null? float.Parse(scale) : 1f
                        );
                        break;
                    case "Pole":
                    case "ColorCube":
                        oinfo.m_Renderer = new RendererConfig(
                            xr.GetAttribute("border"),
                            xr.GetAttribute("fill")
                        );
                        break;
                    case "Player":
                        oinfo.m_Renderer = new RendererConfig(
                            scale: scale != null? float.Parse(scale) : 1f,
                            animation: xr.GetAttribute("animation")
                        );
                        break;
                    case "Luigi":
                        oinfo.m_Renderer = new RendererConfig(
                            scale: scale != null? float.Parse(scale) : 1f
                        );
                        break;
                    case "ChainedChomp":
                    case "Goomboss":
                    case "Tree":
                    case "Painting":
                    case "UnchainedChomp":
                    case "Fish":
                    case "Butterfly":
                    case "Star":
                    case "BowserSkyPlatform":
                    case "BigSnowman":
                    case "Toxbox":
                    case "Pokey":
                    case "FlPuzzle":
                    case "FlameThrower":
                    case "C1Trap":
                    case "Wiggler":
                    case "Koopa":
                    case "KoopaShell":
                        // no params
                        break;
                    default:
                        throw new Exception("Unknown renderer for '" + oinfo.m_Name + "' (id = " + oinfo.m_ID + ").");
                }

                List<ObjectInfo.ParamInfo> paramlist = new List<ObjectInfo.ParamInfo>();
                while (xr.ReadToNextSibling("param"))
                {
                    ObjectInfo.ParamInfo pinfo = new ObjectInfo.ParamInfo();

                    xr.ReadToFollowing("name");
                    pinfo.m_Name = xr.ReadElementContentAsString();

                    xr.ReadToFollowing("offset");
                    temp = xr.ReadElementContentAsString();
                    int.TryParse(temp, out pinfo.m_Offset);
                    xr.ReadToFollowing("length");
                    temp = xr.ReadElementContentAsString();
                    int.TryParse(temp, out pinfo.m_Length);

                    xr.ReadToFollowing("type");
                    pinfo.m_Type = xr.ReadElementContentAsString();
                    xr.ReadToFollowing("values");
                    pinfo.m_Values = xr.ReadElementContentAsString();

                    xr.ReadToFollowing("description");
                    pinfo.m_Description = xr.ReadElementContentAsString();

                    paramlist.Add(pinfo);
                }
                oinfo.m_ParamInfo = paramlist.ToArray();
            }

            xr.Close();
            fs.Close();
        }

        public static void LoadFallback()
        {
            StringReader sr = new StringReader(Properties.Resources.obj_list);

            String curline;
            Regex lineregex = new Regex("0x([\\dabcdef]+) == (.*?) \\(0x([\\dabcdef]+)\\)");
            
            while ((curline = sr.ReadLine()) != null)
            {
                Match stuff = lineregex.Match(curline);

                int id = int.Parse(stuff.Groups[1].Value, NumberStyles.HexNumber);
                ObjectInfo oinfo = m_ObjectInfo[id];

                oinfo.m_ID = (ushort)id;
                oinfo.m_Name = stuff.Groups[2].Value;
                oinfo.m_InternalName = stuff.Groups[2].Value;
                oinfo.m_ActorID = ushort.Parse(stuff.Groups[3].Value, NumberStyles.HexNumber);
                
                oinfo.m_Description = "";
                oinfo.m_BankRequirement = 2;
                oinfo.m_ParamInfo = new ObjectInfo.ParamInfo[0];
            }

            sr.Close();
        }

        public static void Update(bool force)
        {
            string ts = force ? "" : "?ts=" + m_Timestamp.ToString();
            m_WebClient.DownloadStringAsync(new Uri(Program.ServerURL + "download_objdb.php" + ts));
        }
    }
}
