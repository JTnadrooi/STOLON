using AsitLib;
using STOLON.CLI;
#pragma warning disable CS0162 // Unreachable code detected

namespace STOLON.CLI
{
    public static class Program
    {
        public const bool ALWAYS_VERBOSE = true;
        public const bool DEBUG_MODE = false;

        public static void Main(string[] args)
        {
            using CLI cli = new CLI(ALWAYS_VERBOSE || args.Contains("-v"));

            if (args.Length == 0)
            {
                CLI.Instance.Debug.Log("no startup arguments given, awaiting arguments.");
                while (true)
                {
                    Console.Write("> ");
                    string[] newArgs = CommandHelpers.SplitArgs(Console.ReadLine()!);
                    if (DEBUG_MODE)
                        CLI.Instance.Execute(newArgs);
                    else
                        try
                        {
                            CLI.Instance.Execute(newArgs);
                        }
                        catch (Exception e)
                        {
                            CLI.Instance.Debug.Log("command failed: " + e.Message);
                        }
                }
            }
            else CLI.Instance.Execute(args);
        }
    }
}
