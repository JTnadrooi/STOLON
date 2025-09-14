using AsitLib;
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
            object[] methodArguments = new object[args.Length];
            for (int i = 0; i < args.Length; i++) methodArguments[i] = Convert.ChangeType(args[i], expected[i].ParameterType);
            return methodArguments;
        }
        public static string[] SplitArgs(string str)
        {
            var matches = new Regex(@"(?:\""(.*?)\"")|(\S+)").Matches(str.Trim());

            var args = new List<string>();

            foreach (Match match in matches)
            {
                var quotedArg = match.Groups[1].Value;
                var regularArg = match.Groups[2].Value;

                var arg = !string.IsNullOrEmpty(quotedArg) ? quotedArg : regularArg;
                args.Add(arg);
            }

            return args.ToArray();
        }

        public static string[] GetTags(string[] args)
            => args.Where(s => s.StartsWith('-')).ToArray();
    }
}
