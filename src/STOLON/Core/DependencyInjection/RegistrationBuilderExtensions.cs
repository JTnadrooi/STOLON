using Autofac.Builder;
using Autofac.Features.Scanning;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;

namespace STOLON
{
    public record class RegisteredTypeInfo(Type RegisteredType, Type MarkerInterface);

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

        public static Type[] GetImplementedBaseClasses(Type type)
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
                Type[] interfaces = { typeof(ISingletonDependency), typeof(IScopedDependency), typeof(ITransientDependency) };
                Type[] assemblyTypes = assembly.GetTypes();

                foreach (Type lifetimeMarkerInterface in interfaces)
                    foreach (Type type in assemblyTypes)
                    {
                        if (!type.IsClass || type.IsAbstract) continue;

                        Type[] implementedInterfaces = type.GetInterfaces().Where(i => interfaces.Contains(i)).ToArray();
                        if (!implementedInterfaces.Contains(lifetimeMarkerInterface)) continue;
                        if (implementedInterfaces.Length > 1) throw new InvalidOperationException();
                        if (type.Namespace?.StartsWith("System") == true) continue;

                        registeredTypes.Add(new RegisteredTypeInfo(type, lifetimeMarkerInterface));

                        IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration = builder.RegisterType(type)
                            .AsImplemented()
                            .AsSelf();

                        if (lifetimeMarkerInterface == typeof(IScopedDependency)) registration.InstancePerLifetimeScope();
                        else if (lifetimeMarkerInterface == typeof(ITransientDependency)) registration.InstancePerDependency();
                        else registration.SingleInstance();
                    }
            }

            return registeredTypes.ToArray();
        }
    }
}
