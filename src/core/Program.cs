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
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using System.IO;
using Serilog;
using System;
using System.Runtime.InteropServices;
using SM64DSe.core.Api;

namespace SM64DSe
{
    static class Program
    {
        
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();
        
        // APPLICATION INFORMATION
        public static string AppTitle = "SM64DS Editor ULTIMATE";
        public static string AppVersion = "v3.0.0";
        public static string AppDate = "Dec 02, 2021";
        public static string ServerURL = "http://kuribo64.net/";

        public static RomEditor romEditor;
        
        // THE FOLLOWING ELEMENTS ARE DEPRECATED AND SHOULD NOT BE ACCESS DIRECTLY
        public static NitroROM m_ROM
        {
            get => romEditor.DangerousGetRom();
            set => throw new Exception("Program.m_ROM cannot be changed. Use Program.romEditor");
        }
        public static string m_ROMPath;
        public static bool m_IsROMFolder;
        public static string m_ROMBasePath;
        public static string m_ROMPatchPath;
        public static string m_ROMConversionPath;
        public static string m_ROMBuildPath;
        
        // 
        public static List<LevelEditorForm> m_LevelEditors;

        [STAThread]
        static void Main(string[] args)
        {
#if DEBUG
            AllocConsole();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
#endif
            Log.Information("SM64DSe-Ultimate");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            romEditor = new RomEditor(args);
            if (romEditor.isOpen) {
                Application.Run(new MainForm());
            }
            else
            {
                Application.Run(new MainForm());
                //TODO: show recent projects
            }
#if DEBUG
            FreeConsole();
#endif
        }
    }
}
