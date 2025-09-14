using AsitLib;
using STOLON.Cmd;
#pragma warning disable CS0162 // Unreachable code detected

namespace STOLON.Installer
{
    public static class Program
    {
        public const bool ALWAYS_VERBOSE = true;
        public const bool DEBUG_MODE = false;

        public static void Main(string[] args)
        {
            using CommandHandler cmdHandler = new CommandHandler(ALWAYS_VERBOSE || args.Contains("-v"));

            if (args.Length == 0)
            {
                CommandHandler.Instance.Debug.Log("no startup arguments given, awaiting arguments.");
                while (true)
                {
                    Console.Write("> ");
                    string[] newArgs = CommandHelpers.SplitArgs(Console.ReadLine()!);
                    if (DEBUG_MODE)
                        CommandHandler.Instance.Execute(newArgs);
                    else
                        try
                        {
                            CommandHandler.Instance.Execute(newArgs);
                        }
                        catch (Exception e)
                        {
                            CommandHandler.Instance.Debug.Log("command failed: " + e.Message);
                        }
                }
            }
            else CommandHandler.Instance.Execute(args);
        }
    }
}
