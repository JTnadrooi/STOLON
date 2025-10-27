using AsitLib;
using STOLON.CLI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using STOLON.CLI.Command;

namespace STOLON.CLI
{
    public static class CommandHelpers
    {
        public static void ValidateArgument(object? argument, ParameterInfo parameter)
        {
            IEnumerable<ValidationAttribute> validationAttributes = parameter.GetCustomAttributes<ValidationAttribute>();
            foreach (ValidationAttribute attribute in validationAttributes)
            {
                ValidationResult? result = attribute.GetValidationResult(argument, new ValidationContext(argument!) { MemberName = parameter.Name });
                if (result != ValidationResult.Success) throw new ArgumentException($"Argument '{parameter.Name}' is invalid: {result.ErrorMessage}");
            }
        }

        public static object? ParseArgument(object? value, Type conversionType)
        {
            if (value == null) return null;
            if (conversionType.IsEnum)
                if (value is string s) return Enum.Parse(conversionType, s.Split('.')[^1], ignoreCase: true);
                else return Enum.ToObject(conversionType, value);
            return Convert.ChangeType(value, conversionType);
        }

        public static object?[] ParseArguments(string[] args, ParameterInfo[] expected)
        {
            object?[] methodArguments = new object?[expected.Length];

            for (int i = 0; i < expected.Length; i++)
            {
                if (i < args.Length)
                {
                    methodArguments[i] = (args[i] == "_" && expected[i].HasDefaultValue) ? expected[i].DefaultValue : ParseArgument(args[i], expected[i].ParameterType);
                    ValidateArgument(methodArguments[i], expected[i]);
                }
                else if (expected[i].HasDefaultValue) methodArguments[i] = expected[i].DefaultValue;
                else throw new ArgumentException($"Missing argument for parameter {expected[i].Name}.");
            }

            return methodArguments;
        }

        public static string[] SplitArgs(string str)
        {
            //str = Regex.Replace(str, @"(?<!\\)#.*", string.Empty);
            MatchCollection matches = new Regex(@"(?:\""(.*?)\"")|(\S+)").Matches(str.Trim()); // handles args between 
            List<string> args = new List<string>();
            foreach (Match match in matches)
                args.Add(!string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value);
            return args.ToArray();
        }

        public readonly record struct ArgumentsInfo(string CmdName, string[] Args, HashSet<string> Flags);

        public static ArgumentsInfo RefineArguments(string[] args)
        {
            HashSet<string> merged = new HashSet<string>(CLI.Instance.GlobalFlags);
            List<string> positionalArgs = new List<string>();
            bool afterSeparator = false;
            for (int i = 1; i < args.Length; i++)
                if (afterSeparator) positionalArgs.Add(args[i]);
                else if (args[i] == "--") afterSeparator = true;
                else if (args[i].StartsWith("--")) merged.Add(args[i][2..]);
                else if (args[i].StartsWith("-")) merged.Add(args[i][1..]);
                else positionalArgs.Add(args[i]);
            return new ArgumentsInfo(args[0].ToLower(), positionalArgs.ToArray(), merged);
        }
    }
}
