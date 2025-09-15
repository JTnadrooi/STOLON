using AsitLib;
using AsitLib.Debug;
using STOLON.CLI;
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
using static STOLON.CLI.CommandHelpers;

namespace STOLON.CLI
{
    public class CLI : IDisposable
    {
        private bool disposedValue;

        public FrozenDictionary<string, CommandInfo> Commands { get; }
        public DebugStream Debug { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CLI Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


        public CLI(bool verbose)
        {
            Debug = new DebugStream(header: "STOLON.CMD") { Silent = !verbose };
            Instance = this;

            Debug.Log(">creating handler.");
            Dictionary<string, CommandInfo> commandInfos = new Dictionary<string, CommandInfo>();
            List<Type> commandProviderTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(CommandProvider)) && !t.IsAbstract).ToList();
            Debug.Log($">found {commandProviderTypes.Count} command provider types, scanning.");
            foreach (Type providerType in commandProviderTypes)
            {
                CommandProvider providerInstance = (CommandProvider)Activator.CreateInstance(providerType)!;

                MethodInfo[] commandMethods = providerType.GetMethods();
                foreach (MethodInfo methodInfo in commandMethods)
                    if (methodInfo.GetCustomAttribute<CommandAttribute>() is CommandAttribute attribute)
                    {
                        List<string> ids = new List<string>() { attribute.IdOverride?.ToLower() ?? methodInfo.Name.ToLower() };
                        if (attribute.Aliases != null) ids.AddRange(attribute.Aliases);
                        string[] idArray = ids.ToArray();
                        for (int i = 0; i < idArray.Length; i++)
                        {
                            string alias = idArray[i];
                            if (commandInfos.ContainsKey(alias))
                                throw new InvalidOperationException($"Command with '{alias}' is already registered. Overloads are not supported.");
                            else commandInfos.Add(alias, new CommandInfo(idArray, attribute.Description, methodInfo, providerInstance));
                        }
                    }
            }
            Debug.Log($"<found {commandInfos.Count} commands.");

            Commands = commandInfos.ToFrozenDictionary();

            Debug.Log($"<command handler created succesfully.");
            Console.WriteLine(Commands.ToJoinedString(",\n"));
        }

        public void Execute(string[] arguments) => Execute(CommandHelpers.RefineArguments(arguments));
        public void Execute(ArgumentsInfo arguments)
        {
            if (!Commands.TryGetValue(arguments.CmdName, out CommandInfo? command)) throw new InvalidOperationException($"Command '{arguments.CmdName}' not found.");

            List<IOptionHandler> optionHandlers = new List<IOptionHandler>()
            {
                new VerboseOptionHandler(),
            };

            foreach (IOptionHandler optionHandler in optionHandlers) optionHandler.PreCommand(arguments);

            Debug.Log($"found command with id/alias: '{arguments.CmdName}'.");

            object?[] cmdArgs = CommandHelpers.ParseArguments(arguments.Args, command.MethodInfo.GetParameters());
            Debug.Log($"executing with arguments: {string.Join(", ", cmdArgs)}");
            Debug.Log($"executing with options: {string.Join(", ", arguments.Options)}");

            command.MethodInfo.Invoke(command.Source, cmdArgs);

            foreach (IOptionHandler optionHandler in optionHandlers) optionHandler.PostCommand();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class CommandInfo
    {
        public string[] Ids { get; }
        public string Id => Ids[0];
        public string Description { get; }
        public MethodInfo MethodInfo { get; }
        public CommandProvider Source { get; }

        public CommandInfo(string[] ids, string description, MethodInfo methodInfo, CommandProvider source)
        {
            Ids = ids;
            Description = description;
            MethodInfo = methodInfo;
            Source = source;
        }

        public override string ToString() => $"CommandInfo(Ids: {string.Join(", ", Ids)}, Method: {MethodInfo}, Source: {Source?.ToString()})";
    }

    public sealed class CommandAttribute : Attribute
    {
        public string? IdOverride { get; }
        public string[]? Aliases { get; }
        public string Description { get; }
        public CommandAttribute(string description, string? idOverride = null, string[]? aliases = null)
        {
            IdOverride = idOverride;
            Description = description;
            Aliases = aliases;
        }
    }
}
