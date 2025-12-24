using DiscordRPC.Logging;
using Autofac;
using System.Diagnostics;
using System.Reflection;
using Autofac.Builder;

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

            Type[] interfaces = { typeof(ISingletonDependency), typeof(IScopedDependency), typeof(ITransientDependency) };
            Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();

#if DEBUG
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsInterface)
                {
                    if (t.GetInterfaces().Any(i => interfaces.Contains(i))) throw new InvalidProgramException($"Interface '{t.FullName}' must not inherit from lifetime marker interfaces.");
                }
            }
#endif

            RegisteredTypeInfo[] registeredTypes = builder.RegisterMarkedAssemblyTypes(Assembly.GetExecutingAssembly());

            for (int i = 0; i < registeredTypes.Length; i++)
            {
                Console.WriteLine($"type '{registeredTypes[i].RegisteredType}' for lifetime marker interface '{registeredTypes[i].MarkerInterface}'.");
            }

            _services = STOLON.Services = builder.Build();

            Console.WriteLine($"DI registration completed in {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Stop();
        }

        public static void Main()
        {
            using STOLON game = _services.Resolve<STOLON>();
            game.Run();
        }
    }
}
