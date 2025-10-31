using AsitLib;
using AsitLib.Debug;
using STOLON.CLI;
using STOLON.CLI.Command;
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
    public sealed class CLI : IDisposable
    {
        private bool disposedValue;

        public FrozenDictionary<string, CommandInfo> Commands { get; }
        public FrozenDictionary<string, CommandInfo> UniqueCommands { get; }
        public FrozenDictionary<string, CommandProvider> Providers { get; }
        public FrozenDictionary<string, FlagHandler> FlagHandlers { get; }

        public Configuration Config { get; }

        public HashSet<string> GlobalFlags { get; }

        public CLI(string[] args)
        {
            //throw new Exception(Directory.GetFiles(".\\", "*", SearchOption.AllDirectories).ToJoinedString("\n"));
            static int GetNestedClassDepth(Type type)
            {
                int depth = 0;
                while (type.DeclaringType != null)
                {
                    depth++;
                    type = type.DeclaringType;
                }
                return depth;
            }

            Config = new Configuration();
            GlobalFlags = Config.Get<string[]>("cli.global_flags").ToHashSet();
            STOLON.Debug = Debug = new DebugStream(header: "STOLON.CLI") { Silent = !(GlobalFlags.Contains("v") || args.Contains("-v")) };

            Instance = this;

            Debug.Log(">creating cli.");
            Dictionary<string, CommandInfo> commandInfos = new Dictionary<string, CommandInfo>();
            Dictionary<string, CommandProvider> nestedProviders = new Dictionary<string, CommandProvider>();
            Dictionary<string, CommandInfo> uniqueCommandInfos = new Dictionary<string, CommandInfo>();
            FlagHandlers = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(FlagHandler)) && !t.IsAbstract)
                    .Select(fht => (FlagHandler)Activator.CreateInstance(fht)!)
                    .ToFrozenDictionary(fh => fh.LongId);
            List<Type> commandProviderTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(CommandProvider)) && !t.IsAbstract)
                .OrderBy(t => GetNestedClassDepth(t))
                .ToList();
            Debug.Log($">found {commandProviderTypes.Count} command provider types, scanning.");
            foreach (Type providerType in commandProviderTypes)
            {
                CommandProvider providerInstance = (CommandProvider)Activator.CreateInstance(providerType)!;

                string ResolveNamespaceRecusive(string ns, CommandProvider current)
                {
                    Type? nestedIn = current.GetType().DeclaringType;
                    if (nestedIn == null) return ns;
                    if (nestedIn.BaseType != typeof(CommandProvider)) throw new InvalidOperationException($"Nested provider '{current.GetType()}' is declared inside '{nestedIn}', which does not inherit from CommandProvider.");

                    CommandProvider cmdp = nestedProviders.Values.Where(v => v.GetType() == nestedIn).First();

                    return ResolveNamespaceRecusive(cmdp.Namespace + "-" + ns, cmdp);
                }
                providerInstance.FullNamespace = ResolveNamespaceRecusive(providerInstance.Namespace, providerInstance);

                nestedProviders.Add(providerInstance.FullNamespace, providerInstance);

                MethodInfo[] commandMethods = providerType.GetMethods();
                foreach (MethodInfo methodInfo in commandMethods)
                    if (methodInfo.GetCustomAttribute<CommandAttribute>() is CommandAttribute attribute)
                    {
                        if (methodInfo.ReturnType != typeof(void)) throw new InvalidOperationException("Commands must have a void return type.");

                        string cmdId;
                        if (methodInfo.Name == "_M") cmdId = providerInstance.FullNamespace;
                        else cmdId = (attribute.InheritNamespace ? (providerInstance.FullNamespace + "-") : string.Empty) + (attribute.Id?.ToLower() ?? methodInfo.Name.ToLower());

                        string[] ids = new string[] { cmdId }.Concat(attribute.Aliases ?? Enumerable.Empty<string>()).ToArray();

                        CommandInfo info = new CommandInfo(ids, attribute, methodInfo, providerInstance);
                        uniqueCommandInfos.Add(info.Id, info);
                        for (int i = 0; i < ids.Length; i++)
                        {
                            string alias = ids[i];
                            if (commandInfos.ContainsKey(alias))
                                throw new InvalidOperationException($"Command with '{alias}' is already registered. Overloads are not supported.");
                            else commandInfos.Add(alias, info);
                        }
                    }
            }
            Debug.Log($"<found {commandInfos.Count} commands.");

            Commands = commandInfos.ToFrozenDictionary();
            UniqueCommands = uniqueCommandInfos.ToFrozenDictionary();
            Providers = nestedProviders.ToFrozenDictionary();

            Debug.Log($"<cli created succesfully.");
            //Console.WriteLine(nestedProviders.ToJoinedString(",\n"));
        }
        public void Execute(string str) => Execute(CommandHelpers.SplitArgs(str));
        public void Execute(string[] arguments) => Execute(CommandHelpers.RefineArguments(arguments));
        public void Execute(ArgumentsInfo arguments)
        {
            if (!Commands.TryGetValue(arguments.CmdName, out CommandInfo? command)) throw new InvalidOperationException($"Command '{arguments.CmdName}' not found.");

            Debug.Log($"found command with id/alias: '{arguments.CmdName}'.");

            foreach (FlagHandler flagHandler in FlagHandlers.Values)
                if (flagHandler.ShouldListen(arguments))
                    flagHandler.PreCommand(arguments);

            if (command.HasFlag(CommandFlag.DevOnly) && !IsDev) throw new InvalidOperationException($"Command '{arguments.CmdName}' is dev-only.");

            object?[] cmdArgs = CommandHelpers.ParseArguments(arguments.Args, command.MethodInfo.GetParameters());
            Debug.Log($"executing with arguments: {string.Join(", ", cmdArgs)}");
            Debug.Log($"executing with options: {string.Join(", ", arguments.Flags)}");

            command.MethodInfo.Invoke(command.Provider, cmdArgs);

            foreach (FlagHandler flagHandler in FlagHandlers.Values)
                if (flagHandler.ShouldListen(arguments))
                    flagHandler.PostCommand();
        }

        public void Exit(int exitCode = 0)
        {
            Dispose();
            Environment.Exit(exitCode);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (CommandProvider cmdp in Providers.Values) (cmdp as IDisposable)?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CLI Instance { get; private set; }
        public static DebugStream Debug { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public static TProvider GetProvider<TProvider>() where TProvider : CommandProvider
            => (TProvider)CLI.Instance.Providers.Values.First(p => p.GetType() == typeof(TProvider));

        public const string BUILD_INFO_DIRECTORY = @".buildinfo\";
        private const string RELATIVE_SOURCE_PATH = @".\..\..\src\";

        public static bool IsDev => !CLI.Instance.Config.Get<bool>("cli.ignore_buildinfo") && Directory.Exists(BUILD_INFO_DIRECTORY); // can't be in static().
        public static string? SourcePath => IsDev ? (System.IO.Path.GetFullPath(RELATIVE_SOURCE_PATH)) : null;
        public static string? SourceResourcesPath => SourcePath == null ? null : (SourcePath + @"STOLON\resources\");
    }
}
