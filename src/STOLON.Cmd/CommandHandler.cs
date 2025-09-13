using AsitLib;
using AsitLib.Debug;
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
    public class CommandHandler : IDisposable
    {
        private bool disposedValue;

        public FrozenDictionary<string, CommandInfo> Commands { get; }
        public DebugStream Debug { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static CommandHandler Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public CommandHandler()
        {
            Debug = new DebugStream(header: "STOLON.CMD");
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

        public void Execute(string[] args)
        {
            if (args.Length == 0) throw new InvalidOperationException("Cannot execute any command without arguments. One must be for the command name itself.");

            if (!Commands.TryGetValue(args[0].ToLower(), out CommandInfo? command)) throw new InvalidOperationException($"Command '{args[0]}' not found.");
            Debug.Log($"found command with id/alias: '{args[0].ToLower()}'.");

            string[] parameters = args.Skip(1).ToArray();

            ParameterInfo[] methodParams = command.MethodInfo.GetParameters().ToArray();
            object[] methodArguments = new object[methodParams.Length];

            for (int i = 0; i < methodParams.Length; i++) methodArguments[i] = Convert.ChangeType(parameters[i], methodParams[i].ParameterType);

            Debug.Log($"executing with arguments: {string.Join(", ", methodArguments)}");
            command.MethodInfo.Invoke(command.Source, methodArguments);
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

    public abstract class CommandProvider
    {
        public string Id { get; }
        public CommandProvider(string id) => Id = id;
        public override string ToString() => $"CommandProvider(Id: {Id})";
    }
}
