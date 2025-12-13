using AsitLib;
using STOLON.CLI;
#pragma warning disable CS0162 // Unreachable code detected

namespace STOLON.CLI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CLI cli = new CLI(args);

            if (args.Length == 0)
            {
                CLI.Logger.Log("no startup arguments given, awaiting arguments.");
                while (true)
                {
                    Console.Write("> ");
                    if (CLI.Instance.Config.GetBool("cli.catch_errors"))
                        try
                        {
                            CLI.Engine.Execute(Console.ReadLine()!).WriteToConsole();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("command failed: " + e.Message);
                        }
                    else
                        CLI.Engine.Execute(Console.ReadLine()!).WriteToConsole();
                }
            }
            else CLI.Engine.Execute(args).WriteToConsole();
        }
    }
}
