using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TinyNvidiaUpdateChecker.Handlers;

internal static partial class ConsoleHelper
{

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AttachConsole(uint dwProcessId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool FreeConsole();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial IntPtr GetConsoleWindow();

    private const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;

    public static bool debuggerAttached = Debugger.IsAttached;

    public static bool consoleAttached = false;

    /// <summary>
    /// Attaches to the calling console (e.g., cmd/PowerShell) or allocates a new console window if needed.
    /// </summary>
    public static void Init(string title)
    {
        if (MainConsole.showUI && !debuggerAttached)
        {
            if (GetConsoleWindow() == IntPtr.Zero)
            {
                bool success = AttachConsole(ATTACH_PARENT_PROCESS);
                consoleAttached = true;

                if (success)
                {
                    ConsoleHelper.WriteLine();
                    MainConsole.noPrompt = true; // no prompt needed, we are in existing console
                }
                else
                {
                    AllocConsole();
                }
            }

            Console.Title = title;

            if (!MainConsole.debug)
            {
                GenericHandler.DisableQuickEdit();
            }
        }
        else if (!MainConsole.showUI && !debuggerAttached)
        {
            FreeConsole();
        }
    }

    public static void Write(string value = "")
    {
        if (!MainConsole.showUI) return;
        if (!consoleAttached) AttachConsole();
        if (debuggerAttached) AllocConsole();
        Console.Write(value);
    }

    public static void WriteLine(string value = "")
    {
        if (!MainConsole.showUI) return;
        if (!consoleAttached) AttachConsole();
        if (debuggerAttached) AllocConsole();
        Console.WriteLine(value);
    }

    private static void AttachConsole()
    {
        if (GetConsoleWindow() == IntPtr.Zero)
        {
            bool success = AttachConsole(ATTACH_PARENT_PROCESS);
            consoleAttached = true;

            if (success)
            {
                WriteLine();
            }
            else
            {
                AllocConsole();
            }
        }
    }
}