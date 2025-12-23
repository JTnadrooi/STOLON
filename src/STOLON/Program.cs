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
                   .As<IRichLogger>()
                   .SingleInstance()
                   .OnActivating(e => e.Instance.Silent = false);

            //builder.RegisterType<Environment>().SingleInstance();
            //builder.RegisterType<Configuration>().As<IConfiguration>().SingleInstance();
            //builder.RegisterType<AudioEngine>().SingleInstance();
            //builder.RegisterType<InputManager>().As<IInputManager>().SingleInstance();
            //builder.RegisterType<TaskHeap>().SingleInstance();
            //builder.RegisterType<Interface>().SingleInstance();
            //builder.RegisterType<SceneManager>().SingleInstance();
            //builder.RegisterType<STOLON>().SingleInstance();

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

            foreach (Type serviceInterface in interfaces)
                foreach (Type type in assemblyTypes)
                {
                    if (!type.IsClass || type.IsAbstract) continue;

                    Type[] implementedInterfaces = type.GetInterfaces().Where(i => interfaces.Contains(i)).ToArray();
                    if (!implementedInterfaces.Contains(serviceInterface)) continue;
                    if (implementedInterfaces.Length > 1) throw new InvalidOperationException();
                    if (type.Namespace?.StartsWith("System") == true) continue;

                    List<Type> baseTypes = new List<Type>();
                    Type current = type.BaseType!; // not null bc of the IsClass above.
                    while (current != typeof(object))
                    {
                        baseTypes.Add(current);
                        current = current.BaseType!;
                    }

                    Console.WriteLine($"type '{type}' for interface '{serviceInterface}'.");

                    IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration = builder.RegisterType(type)
                        .AsImplementedInterfaces()
                        .As(baseTypes.ToArray())
                        .AsSelf();

                    if (serviceInterface == typeof(IScopedDependency)) registration.InstancePerLifetimeScope();
                    else if (serviceInterface == typeof(ITransientDependency)) registration.InstancePerDependency();
                    else registration.SingleInstance();
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
