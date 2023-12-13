using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SPTarkovLauncher.Config;

namespace SPTarkovLauncher;

internal static class Program
{
    private const string LauncherProcess = "Aki.Launcher";
    private const string LauncherFileName = "Aki.Launcher.exe";
    private const string ServerProcess = "Aki.Server";
    private const string ServerFileName = "Aki.Server.exe";
    private const string TarkovProcessName = "EscapeFromTarkov";

    private static readonly IConfigManager Config = new RegistryConfigManager(@"Software\SPTarkovLauncher");

    public static async Task Main(string[] args)
    {
        try
        {
            await Worker();
        }
        catch (Exception e)
        {
            Console.WriteLine();
            Console.WriteLine("An error occurred");
            Console.WriteLine(e.Message);
            Console.WriteLine("Press 'e' for extended error information, or any other key to exit.");
            
            if (Console.ReadKey().KeyChar is 'e')
            {
                Console.WriteLine();
                Console.WriteLine(e.StackTrace);
                
                Console.WriteLine();
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
    }

    private static async Task Worker()
    {
        // Focus existing Tarkov instance and exit.
        if (ProcessHelper.TryGet(TarkovProcessName, out Process tarkovProcess))
        {
            Console.WriteLine("Tarkov is already running.");
            ProcessHelper.FocusProcessWindow(tarkovProcess);
            return;
        }

        string spTarkovPath = GetInstallationPath();
        bool isLauncherRunning = ProcessHelper.TryGet(LauncherProcess, out Process launcherProcess);
        bool isServerRunning = ProcessHelper.IsRunning(ServerProcess);

        // Focus the launcher window and exit.
        if (isLauncherRunning && isServerRunning)
        {
            Console.WriteLine("Focusing existing launcher instance.");
            ProcessHelper.FocusProcessWindow(launcherProcess);
            return;
        }

        // Start the launcher for use with the existing server instance.
        if (isServerRunning)
        {
            ProcessHelper.Launch(spTarkovPath, LauncherFileName);
            return;
        }

        // The launcher doesn't automatically detect a new server instance, better to restart it.
        if (isLauncherRunning)
        {
            ProcessHelper.Close(LauncherProcess);
        }

        ProcessHelper.Launch(spTarkovPath, ServerFileName);

        // Allow time for the server to spin up.
        await Task.Delay(3000);

        ProcessHelper.Launch(spTarkovPath, LauncherFileName);
    }

    private static string GetInstallationPath()
    {
        if (Config.Load(ConfigKeys.SPTarkovPath, out string path) && IsValidDirectory(path))
        {
            return path;
        }

        path = QueryUserForInstallationPath();
        Config.Save(ConfigKeys.SPTarkovPath, path);
        return path;
    }

    private static string QueryUserForInstallationPath()
    {
        string path;
        do
        {
            Console.WriteLine("Please enter the path to your SPTarkov installation");
            Console.WriteLine(@"e.g. C:\Games\SPTarkov");
            Console.Write("Path: ");
            path = Console.ReadLine();
            Console.WriteLine();
        } while (!IsValidDirectory(path));

        return path;
    }

    private static bool IsValidDirectory(string spTarkovPath)
    {
        if (!Directory.Exists(spTarkovPath))
        {
            Console.WriteLine($"Directory doesn't exist: \"{spTarkovPath}\"");
            return false;
        }

        string[] requiredFileNames = [LauncherFileName, ServerFileName];
        string[] actualFileNames = new DirectoryInfo(spTarkovPath)
            .GetFiles()
            .Select(x => x.Name)
            .ToArray();


        foreach (string required in requiredFileNames)
        {
            if (!actualFileNames.Contains(required))
            {
                Console.WriteLine($"File doesn't exist: \"{required}\"");
                return false;
            }
        }

        return true;
    }
}