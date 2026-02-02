using Autofac.Builder;
using Autofac.Features.Scanning;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;

namespace STOLON
{
    public record class RegisteredTypeInfo(Type RegisteredType, ServiceLifetime Lifetime);

    public static class RegistrationBuilderExtensions
    {
        public static IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> AsImplementedBaseClasses<TLimit, TConcreteActivatorData>(this IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> registration)
            where TConcreteActivatorData : IConcreteActivatorData
        {
            return registration.As(GetImplementedBaseClasses(registration.ActivatorData.Activator.LimitType));
        }

        public static IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> AsImplemented<TLimit, TConcreteActivatorData>(this IRegistrationBuilder<TLimit, TConcreteActivatorData, SingleRegistrationStyle> registration)
            where TConcreteActivatorData : IConcreteActivatorData
        {
            return registration.AsImplementedInterfaces().AsImplementedBaseClasses();
        }

        public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle>
            AsImplementedBaseClasses<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.As(t => GetImplementedBaseClasses(t));
        }

        public static IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle>
            AsImplemented<TLimit>(this IRegistrationBuilder<TLimit, ScanningActivatorData, DynamicRegistrationStyle> registration)
        {
            return registration.AsImplementedInterfaces().AsImplementedBaseClasses();
        }

        private static Type[] GetImplementedBaseClasses(Type type)
        {
            List<Type> baseTypes = new List<Type>();

            Type? current = type.BaseType;
            while (current is not null && current != typeof(object))
            {
                baseTypes.Add(current!);
                current = current.BaseType;
            }

            return baseTypes.ToArray();
        }

        public static RegisteredTypeInfo[] RegisterMarkedAssemblyTypes(this ContainerBuilder builder, Assembly assembly) => RegisterMarkedAssemblyTypes(builder, [assembly]);
        public static RegisteredTypeInfo[] RegisterMarkedAssemblyTypes(this ContainerBuilder builder, Assembly[] assemblies)
        {
            List<RegisteredTypeInfo> registeredTypes = new List<RegisteredTypeInfo>();

            foreach (Assembly assembly in assemblies)
            {
                Type[] assemblyTypes = assembly.GetTypes();

                foreach (Type type in assemblyTypes)
                {
                    if (!type.IsClass || type.IsAbstract) continue;

                    DependencyAttribute? attribute = type.GetCustomAttribute<DependencyAttribute>();

                    Type? baseType = type.BaseType;

                    while (attribute is null)
                    {
                        if (baseType is not null)
                        {
                            attribute = baseType.GetCustomAttribute<DependencyAttribute>();
                            baseType = baseType.BaseType;
                        }
                        else break;
                    }

                    if (attribute is null) continue;
                    if ((type.Namespace?.StartsWith("System")).GetValueOrDefault()) continue;

                    registeredTypes.Add(new RegisteredTypeInfo(type, attribute.Lifetime));

                    IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration = builder.RegisterType(type)
                        .AsImplemented()
                        .AsSelf();

                    switch (attribute.Lifetime)
                    {
                        case ServiceLifetime.Singleton: registration.SingleInstance(); break;
                        case ServiceLifetime.Scoped: registration.InstancePerLifetimeScope(); break;
                        case ServiceLifetime.Transient: registration.InstancePerDependency(); break;
                        default: throw new InvalidProgramException($"{type}' has an invalid lifetime '{attribute.Lifetime}'.");
                    }
                }
            }

            return registeredTypes.ToArray();
        }
    }
}
