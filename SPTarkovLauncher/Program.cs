using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SPTarkovLauncher;

internal static class Program
{
    private const string LauncherProcess = "Aki.Launcher";
    private const string LauncherFileName = "Aki.Launcher.exe";
    private const string ServerProcess = "Aki.Server";
    private const string ServerFileName = "Aki.Server.exe";
    private const string TarkovProcessName = "EscapeFromTarkov";

    public static async Task Main(string[] args)
    {
        try
        {
            await ExecuteProgramLogic();
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("Please ensure SPTarkovLauncher is placed in your SPTarkov folder.");
            DisplayException(e);
        }
        // Ensure any unexpected errors are displayed to the user.
        catch (Exception e)
        {
            DisplayException(e);
        }
    }

    private static async Task ExecuteProgramLogic()
    {
        // Focus existing Tarkov instance and exit.
        if (ProcessHelper.TryGet(TarkovProcessName, out Process tarkovProcess))
        {
            Console.WriteLine("Tarkov is already running.");
            ProcessHelper.FocusProcessWindow(tarkovProcess);
            return;
        }

        AssertCorrectDirectory();
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
            ProcessHelper.LaunchFromWorkingDirectory(LauncherFileName);
            return;
        }

        // The launcher doesn't automatically detect a new server instance, better to restart it.
        if (isLauncherRunning)
        {
            ProcessHelper.Close(LauncherProcess);
        }

        ProcessHelper.LaunchFromWorkingDirectory(ServerFileName);

        // TODO: Read server output until "server is running".
        // Allow time for the server to spin up.
        await Task.Delay(3000);

        ProcessHelper.LaunchFromWorkingDirectory(LauncherFileName);
    }

    private static void DisplayException(Exception e)
    {
        Console.WriteLine($"{e.GetType()}: {e.Message}");
        Console.WriteLine("Press 'e' for extended error information, or any other key to exit.");

        if (Console.ReadKey(true).KeyChar is 'e')
        {
            Console.WriteLine();
            Console.WriteLine(e);

            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey(true);
        }
    }

    private static void AssertCorrectDirectory()
    {
        string[] requiredFileNames = [LauncherFileName, ServerFileName];
        string[] actualFileNames = new DirectoryInfo(Directory.GetCurrentDirectory())
            .GetFiles()
            .Select(x => x.Name)
            .ToArray();

        foreach (string required in requiredFileNames)
        {
            if (!actualFileNames.Contains(required))
            {
                throw new FileNotFoundException($"Missing file \"{required}\".");
            }
        }
    }
}