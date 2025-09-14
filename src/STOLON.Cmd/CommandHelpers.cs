using AsitLib;
using STOLON.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STOLON.Cmd
{
    public static class CommandHelpers
    {
        //public static (object?[] cmdArgs, string[] generalArgs) ParseArguments(string args, ParameterInfo[] expected) => ParseArguments(SplitArgs(args), )
        public static object?[] ParseArguments(string[] args, ParameterInfo[] expected)
        {
            object?[] methodArguments = new object?[expected.Length];

            for (int i = 0; i < expected.Length; i++)
            {
                if (i < args.Length) methodArguments[i] = Convert.ChangeType(args[i], expected[i].ParameterType);
                else if (expected[i].HasDefaultValue) methodArguments[i] = expected[i].DefaultValue;
                else throw new ArgumentException($"Missing argument for parameter {expected[i].Name}.");
            }

            return methodArguments;
        }

        public static string[] SplitArgs(string str)
        {
            MatchCollection matches = new Regex(@"(?:\""(.*?)\"")|(\S+)").Matches(str.Trim());
            List<string> args = new List<string>();
            foreach (Match match in matches)
                args.Add(!string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value);
            return args.ToArray();
        }

        public readonly record struct ArgumentsInfo(string CmdName, string[] Args, HashSet<string> Options);

        public static ArgumentsInfo RefineArguments(string[] args)
            => new ArgumentsInfo(args[0].ToLower(), args.Skip(1).Where(s => !s.StartsWith('-')).ToArray(), args.Skip(1).Where(s => s.StartsWith('-')).ToHashSet());
    }
}
