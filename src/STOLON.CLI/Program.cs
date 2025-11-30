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
                            CLI.Instance.Execute(Console.ReadLine()!);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("command failed: " + e.Message);
                        }
                    else
                        CLI.Instance.Execute(Console.ReadLine()!);
                }
            }
            else CLI.Instance.Execute(args);
        }
    }
}
