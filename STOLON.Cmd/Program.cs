using AsitLib;
using AsitLib.Debug;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.Installer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using CommandHandler cmdHandler = new CommandHandler();

            if (args.Length == 0)
            {
                cmdHandler.Debug.Log("no startup arguments given, awaiting arguments.");
                while (true)
                {
                    Console.Write("> ");
                    string[] newArgs = Console.ReadLine()?.Trim().Split(" ") ?? throw new Exception();
                    cmdHandler.Execute(newArgs);
                }
            }
            else cmdHandler.Execute(args);
        }
    }

    public class CommandHandler : IDisposable
    {
        private bool disposedValue;

        public FrozenDictionary<string, CommandInfo> Commands { get; }
        public DebugStream Debug { get; }

        public CommandHandler()
        {
            Debug = new DebugStream(header: "STOLON.CMD");

            Debug.Log(">creating handler.");
            List<CommandInfo> commandInfos = new List<CommandInfo>();
            List<Type> commandProviderTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(CommandProvider)) && !t.IsAbstract).ToList();
            Debug.Log($">found {commandProviderTypes.Count} command provider types, scanning.");
            foreach (var providerType in commandProviderTypes)
            {
                CommandProvider providerInstance = (CommandProvider)Activator.CreateInstance(providerType)!;
                List<MethodInfo> commandMethods = providerType.GetMethods().Where(m => m.GetCustomAttribute<CommandAttribute>() != null).ToList();
                foreach (MethodInfo method in commandMethods)
                    commandInfos.Add(new CommandInfo(
                        method.Name.ToLower(),
                        method,
                        providerInstance
                    ));
            }
            Debug.Log($"<found {commandInfos.Count} commands.");

            Commands = commandInfos.ToFrozenDictionary(item => item.Id);

            Debug.Log($"<command handler created succesfully.");
            Console.WriteLine(Commands.ToJoinedString(",\n"));
        }

        public void Execute(string[] args)
        {
            if (args.Length == 0) throw new InvalidOperationException("Cannot execute any command without arguments. One must be for the command name itself.");

            if (!Commands.TryGetValue(args[0].ToLower(), out CommandInfo? command)) throw new InvalidOperationException($"Command '{args[0]}' not found.");
            Debug.Log($"found command with id: '{command.Id}'.");

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

    public record class CommandInfo(string Id, MethodInfo MethodInfo, CommandProvider Source)
    {
        public override string ToString() => $"CommandInfo(Id: {Id}, Method: {MethodInfo}, Source: {Source?.ToString()})";
    }

    public sealed class CommandAttribute : Attribute { }
    public abstract class CommandProvider { }
    public sealed class StolonCommandProvider : CommandProvider
    {
        [Command]
        public void Add(int a, int b) => Console.WriteLine(a + b);
        //[Command]
        //public int Add(int a, int b, int c) => a + b + c;
        [Command]
        public void Greet(string name) => Console.WriteLine($"Hello, {name}!");
    }
}
