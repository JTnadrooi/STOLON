using AsitLib;
using AsitLib.CommandLine;
using AsitLib.Diagnostics;
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

namespace STOLON.CLI
{
    public sealed class DevActionHook : ActionHook
    {
        public DevActionHook() : base("dev-validate") { }

        public override void PreCommand(CommandContext context)
        {
            if (context.Command is FlaggedCommandInfo cmd)
            {
                if (cmd.HasFlag(CommandFlags.DevOnly))
                {
                    if (!CLI.IsDev)
                    {
                        throw new Exception("This command cannot run without source code/assets.");
                    }
                }
            }
        }
    }

    public sealed class FlaggedCommandInfoFactory : ICommandInfoFactory
    {
        public CommandInfo? Convert(CommandProvider provider, MethodInfo methodInfo, CommandAttribute attribute)
        {
            FlaggedCommandAttribute flaggedCommandAttribute = (FlaggedCommandAttribute)attribute;

            CommandInfo defaultResult = MethodCommandInfo.FromMethod(methodInfo, provider);

            return new FlaggedCommandInfo(defaultResult.Ids.ToArray(), defaultResult.Description, methodInfo)
            {
                Flags = flaggedCommandAttribute.Flags,
                PassingPolicies = flaggedCommandAttribute.PassingPolicies,
                Target = provider,
                Provider = provider,
            };
        }
    }

    public sealed class CLI
    {
        public Configuration Config { get; }

        public HashSet<string> GlobalFlags { get; }

        public CLI(string[] args)
        {
            Config = new Configuration();
            GlobalFlags = Config.Get<string[]>("cli.global_flags").ToHashSet();
            STOLON.Logger = Logger = new Logger(header: "STOLON.CLI") { Silent = !(GlobalFlags.Contains("v") || args.Contains("-v")) };

            InfoFactory = new FlaggedCommandInfoFactory();

            Logger.Log(">creating cli.");

            Engine = new CommandEngine()
                .AddHook(new DevActionHook())
                .AddGlobalOption(Logger.GetVerboseGlobalOption())
                .Populate();

            Instance = this;

            Logger.Log($"<cli created succesfully.");
        }

        public void Exit(int exitCode = 0)
        {
            Environment.Exit(exitCode);
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        /// <summary>
        /// Gets the only <see cref="CLI"/> instance.
        /// </summary>
        public static CLI Instance { get; private set; }
        public static CommandEngine Engine { get; private set; }
        public static Logger Logger { get; private set; }
        public static ICommandInfoFactory InfoFactory { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public const string BUILD_INFO_DIRECTORY = @".buildinfo\";
        private const string RELATIVE_SOURCE_PATH = @".\..\..\src\";
        /// <summary>
        /// Gets if the currently in use dll's are built from a local repo. See the <i>scripts\build.ps1</i> script.
        /// </summary>
        //public static bool IsDev => false;
        public static bool IsDev => !CLI.Instance.Config.Get<bool>("cli.ignore_buildinfo") && Directory.Exists(BUILD_INFO_DIRECTORY); // can't be in static().
        /// <summary>
        /// Gets the absolute path of the <i>src\</i> folder.
        /// </summary>
        public static string SourcePath => IsDev ? (System.IO.Path.GetFullPath(RELATIVE_SOURCE_PATH)) : throw new InvalidOperationException("User is not a dev.");
        /// <summary>
        /// Gets the absolute path of the <i>src\STOLON\resources\</i> folder.
        /// </summary>
        public static string? SourceResourcesPath => SourcePath == null ? null : (SourcePath + @"STOLON\resources\");
    }
}
