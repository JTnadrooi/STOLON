using AsitLib.CommandLine;
using Autofac.Features.ResolveAnything;
using System.Reflection;

namespace STOLON.CLI
{
    public static class Program
    {
        private static readonly IContainer _services;

        static Program()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());

            builder.RegisterType<RichLogger>().As<IRichLogger>().SingleInstance();
            builder.RegisterType<Configuration>().As<IConfiguration>().SingleInstance();

            RegisteredTypeInfo[] registeredTypes = builder.RegisterMarkedAssemblyTypes(Assembly.GetExecutingAssembly());

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeof(CommandProvider).IsAssignableFrom(type)
                    && type != typeof(CommandProvider)
                    && !type.IsAbstract)
                {
                    builder.RegisterType(type).AsImplemented().SingleInstance();
                }
            }

            builder.Register<CommandEngine>(i => new CommandEngine()
                .AddHook(new DevActionHook())
                .AddGlobalOption(i.Resolve<IRichLogger>().GetVerboseGlobalOption())).SingleInstance();

            _services = STOLON.Services = builder.Build();
        }

        public static void Main(string[] args)
        {
            CLI cli = new CLI();

            IRichLogger logger = _services.Resolve<IRichLogger>();
            IConfiguration config = _services.Resolve<IConfiguration>();
            CommandEngine commandEngine = _services.Resolve<CommandEngine>();

            logger.Silent = !(cli.GlobalFlags.Contains("v") || args.Contains("-v"));

            if (args.Length == 0)
            {
                logger.Log("no startup arguments given, awaiting arguments.");
                while (true)
                {
                    Console.Write("> ");
                    if (config.GetBool("cli.catch_errors"))
                        try
                        {
                            commandEngine.Execute(Console.ReadLine()!).WriteToConsole();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("command failed: " + e.Message);
                        }
                    else
                        commandEngine.Execute(Console.ReadLine()!).WriteToConsole();
                }
            }
            else commandEngine.Execute(args).WriteToConsole();
        }
    }
}
