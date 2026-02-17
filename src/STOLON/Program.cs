using AsitLib.CommandLine;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using DiscordRPC.Logging;
using System.Diagnostics;
using System.Reflection;

namespace STOLON
{
    public static class Program
    {
        private static IContainer _services;

        static Program()
        {
            ContainerBuilder builder = new ContainerBuilder();

            Stopwatch stopwatch = Stopwatch.StartNew();

            builder.RegisterType<RichLogger>()
                   .AsImplemented()
                   .SingleInstance()
                   .OnActivating(e => e.Instance.Silent = false);

            Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();

            RegisteredTypeInfo[] registeredTypes = builder.RegisterMarkedAssemblyTypes(Assembly.GetExecutingAssembly());

            for (int i = 0; i < registeredTypes.Length; i++)
            {
                Console.WriteLine($"registered type '{registeredTypes[i].RegisteredType}' with lifetime '{registeredTypes[i].Lifetime}'.");
            }

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                if (typeof(CommandProvider).IsAssignableFrom(type)
                    && type != typeof(CommandProvider)
                    && !type.IsAbstract)
                {
                    builder.RegisterType(type).As(type).AsImplemented().SingleInstance();
                }

            builder.Register<Random>(i => new Random()).SingleInstance();
            builder.Register<CommandEngine>(i => new CommandEngine()).SingleInstance().OnActivated(e => // can be better i think? test later
            {
                e.Instance.Populate(activator: t =>
                {
                    return (CommandProvider)STOLON.Services.Resolve(t);
                });
            });

            _services = STOLON.Services = builder.Build();
            stopwatch.Stop();

            Console.WriteLine($"DI registration completed in {stopwatch.ElapsedMilliseconds} ms.");
        }

        public static void Main()
        {
            using STOLON game = _services.Resolve<STOLON>();
            game.Run();
        }
    }
}
