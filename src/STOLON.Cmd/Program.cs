using AsitLib;
using AsitLib.Debug;
using STOLON.Cmd;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.Installer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using CommandHandler cmdHandler = new CommandHandler(args.Contains("-s"));

            if (args.Length == 0)
            {
                CommandHandler.Instance.Debug.Log("no startup arguments given, awaiting arguments.");
                while (true)
                {
                    Console.Write("> ");
                    string[] newArgs = CommandHelpers.SplitArgs(Console.ReadLine()!);
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
