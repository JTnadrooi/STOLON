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
        public FrozenDictionary<string, CommandInfo> UniqueCommands { get; }
        public FrozenDictionary<string, CommandProvider> Providers { get; }
        public FrozenDictionary<string, FlagHandler> FlagHandlers { get; }

        public DebugStream Debug { get; }
        public CLIConfig Config { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CLI Instance { get; private set; }
        public static string VersionString { get; internal set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public CLI(string[] args)
        {
            Config = new CLIConfig();
            Debug = new DebugStream(header: "STOLON.CMD") { Silent = !(Config.GlobalFlags.Contains("v") || args.Contains("-v")) };
            VersionString = File.ReadAllText(".cli-version");
            Instance = this;

            Debug.Log(">creating cli.");
            Dictionary<string, CommandInfo> commandInfos = new Dictionary<string, CommandInfo>();
            Dictionary<string, CommandProvider> providers = new Dictionary<string, CommandProvider>();
            Dictionary<string, CommandInfo> uniqueCommandInfos = new Dictionary<string, CommandInfo>();
            FlagHandlers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(FlagHandler)) && !t.IsAbstract)
                .Select(fht => (FlagHandler)Activator.CreateInstance(fht)!)
                .ToFrozenDictionary(fh => fh.LongId);
            List<Type> commandProviderTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(CommandProvider)) && !t.IsAbstract).ToList();
            Debug.Log($">found {commandProviderTypes.Count} command provider types, scanning.");
            foreach (Type providerType in commandProviderTypes)
            {
                CommandProvider providerInstance = (CommandProvider)Activator.CreateInstance(providerType)!;
                providers.Add(providerInstance.Namespace, providerInstance);

                MethodInfo[] commandMethods = providerType.GetMethods();
                foreach (MethodInfo methodInfo in commandMethods)
                    if (methodInfo.GetCustomAttribute<CommandAttribute>() is CommandAttribute attribute)
                    {
                        string cmdId;
                        if (methodInfo.Name == "_M") cmdId = providerInstance.Namespace;
                        else cmdId = (attribute.InheritNamespace ? (providerInstance.Namespace + "-") : string.Empty) + (attribute.IdOverride?.ToLower() ?? methodInfo.Name.ToLower());
                        List<string> ids = new List<string>() { cmdId };
                        if (attribute.Aliases != null) ids.AddRange(attribute.Aliases);
                        string[] idArray = ids.ToArray();
                        CommandInfo info = new CommandInfo(idArray, attribute.Description, methodInfo, providerInstance);
                        uniqueCommandInfos.Add(info.Id, info);
                        for (int i = 0; i < idArray.Length; i++)
                        {
                            string alias = idArray[i];
                            if (commandInfos.ContainsKey(alias))
                                throw new InvalidOperationException($"Command with '{alias}' is already registered. Overloads are not supported.");
                            else commandInfos.Add(alias, info);
                        }
                    }
            }
            Debug.Log($"<found {commandInfos.Count} commands.");

            Commands = commandInfos.ToFrozenDictionary();
            UniqueCommands = uniqueCommandInfos.ToFrozenDictionary();
            Providers = providers.ToFrozenDictionary();

            Debug.Log($"<cli created succesfully.");
            //Console.WriteLine(UniqueCommands.ToJoinedString(",\n"));
        }

        public void Execute(string[] arguments) => Execute(CommandHelpers.RefineArguments(arguments));
        public void Execute(ArgumentsInfo arguments)
        {
            if (!Commands.TryGetValue(arguments.CmdName, out CommandInfo? command)) throw new InvalidOperationException($"Command '{arguments.CmdName}' not found.");

            foreach (FlagHandler flagHandler in FlagHandlers.Values)
                if (flagHandler.ShouldListen(arguments))
                    flagHandler.PreCommand(arguments);

            Debug.Log($"found command with id/alias: '{arguments.CmdName}'.");

            object?[] cmdArgs = CommandHelpers.ParseArguments(arguments.Args, command.MethodInfo.GetParameters());
            Debug.Log($"executing with arguments: {string.Join(", ", cmdArgs)}");
            Debug.Log($"executing with options: {string.Join(", ", arguments.Flags)}");

            command.MethodInfo.Invoke(command.Source, cmdArgs);

            foreach (FlagHandler flagHandler in FlagHandlers.Values)
                if (flagHandler.ShouldListen(arguments))
                    flagHandler.PostCommand();
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
        public HashSet<string> Ids { get; }
        public bool HasAliases => Ids.Count > 1;
        public string Id { get; }
        public string Description { get; }
        public MethodInfo MethodInfo { get; }
        public CommandProvider Source { get; }

        public CommandInfo(string[] ids, string description, MethodInfo methodInfo, CommandProvider source)
        {
            Ids = ids.ToHashSet();
            Id = ids[0];
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
        public bool InheritNamespace { get; }
        public CommandAttribute(string description, string? idOverride = null, string[]? aliases = null, bool inheritNamespace = true)
        {
            IdOverride = idOverride;
            Description = description;
            Aliases = aliases;
            InheritNamespace = inheritNamespace;
        }
    }
}
