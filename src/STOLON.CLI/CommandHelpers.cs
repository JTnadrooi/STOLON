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


namespace STOLON.CLI
{
    /// <summary>
    /// Performs operations for validating and parsing the program arguments <see cref="string"/> <see cref="Array"/>.
    /// </summary>
    public static class CommandHelpers
    {
        /// <summary>
        /// Validates if a command <paramref name="argument"/> against the <see cref="ValidationAttribute"/> attributes on the <see cref="ParameterInfo"/>.
        /// </summary>
        /// <param name="argument">The argument to validate.</param>
        /// <param name="parameter">The parameter to get the <see cref="ValidationAttribute"/> attributes from.</param>
        /// <exception cref="ArgumentException">Argument is not valid.</exception>
        public static void ValidateArgument(object? argument, ParameterInfo parameter)
        {
            IEnumerable<ValidationAttribute> validationAttributes = parameter.GetCustomAttributes<ValidationAttribute>();
            foreach (ValidationAttribute attribute in validationAttributes)
            {
                ValidationResult? result = attribute.GetValidationResult(argument, new ValidationContext(argument!) { MemberName = parameter.Name });
                if (result != ValidationResult.Success) throw new ArgumentException($"Argument '{parameter.Name}' is invalid: {result.ErrorMessage}");
            }
        }

        /// <summary>
        /// Parses a value to the target <paramref name="conversionType"/>.
        /// </summary>
        /// <param name="conversionType">The target <see cref="Type"/>.</param>
        /// <returns><paramref name="value"/> parsed to the target <see cref="Type"/>.</returns>
        public static object? ParseArgument(object? value, Type conversionType)
        {
            if (value == null) return null;
            if (conversionType.IsEnum)
                if (value is string s) return Enum.Parse(conversionType, s.Split('.')[^1], ignoreCase: true);
                else return Enum.ToObject(conversionType, value);
            return Convert.ChangeType(value, conversionType);
        }

        /// <summary>
        /// Parses <see cref="string"/> arguments to their expected values.
        /// </summary>
        /// <param name="expected">The parameters the arguments should conform and be parsed to. Obtained from <see cref="MethodBase.GetParameters"/>.</param>
        /// <returns><paramref name="args"/> fit for use in the <see cref="MethodBase"/> the <see cref="ParameterInfo"/> objects are extracted from.</returns>
        /// <exception cref="ArgumentException">Missing argument.</exception>
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

        /// <summary>
        /// Extracts more detailed arguments info from a <see cref="string"/> <see cref="Array"/>.
        /// </summary>
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
