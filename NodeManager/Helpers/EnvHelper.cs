﻿using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace NodeManager.Helpers
{
    internal static class EnvHelper
    {
        public static bool InDesignMode => !(Application.Current is App);

        public static bool Is64BitProc => Environment.Is64BitProcess;
        public static bool Is64BitOS => Environment.Is64BitOperatingSystem;

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RestartAsAdmin()
        {
            // Restart program and run as admin
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            var startInfo = new ProcessStartInfo(exeName)
            {
                Verb = "runas"
            };
            Process.Start(startInfo);
            Application.Current.Shutdown();
        }
    }
}