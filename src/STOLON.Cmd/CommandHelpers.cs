using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.Cmd
{
    public static class CommandHelpers
    {
        public static object?[] ParseArguments(string[] args, ParameterInfo[] expected)
        {
            object[] methodArguments = new object[args.Length];
            for (int i = 0; i < args.Length; i++) methodArguments[i] = Convert.ChangeType(args[i], expected[i].ParameterType);
            return methodArguments;
        }
    }
}
