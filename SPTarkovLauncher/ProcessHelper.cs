using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace SPTarkovLauncher;

internal static class ProcessHelper
{
    private static class Native
    {
        public const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    }

    public static void FocusProcessWindow(Process process)
    {
        if (process.MainWindowHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Attempted to focus a windowless process.");
        }

        Native.ShowWindowAsync(process.MainWindowHandle, Native.SW_RESTORE);
        Native.SetForegroundWindow(process.MainWindowHandle);
    }

    public static bool TryGet(string processName, out Process process)
    {
        Process[] matches = Process.GetProcessesByName(processName);

        if (matches.Length is 0)
        {
            process = null;
            return false;
        }

        process = matches[0];
        return true;
    }

    public static bool IsRunning(string processName) => TryGet(processName, out _);

    public static void LaunchFromWorkingDirectory(string filename)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = Path.Combine(filename)
        };

        Console.WriteLine($"Running {filename}.");
        Process.Start(startInfo);
    }

    public static void Close(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        if (processes.Length is 0)
        {
            return;
        }

        Console.WriteLine($"Closing processes with name \"{processName}\"");
        foreach (Process process in processes)
        {
            process.CloseMainWindow();
            process.Close();
        }
    }
}