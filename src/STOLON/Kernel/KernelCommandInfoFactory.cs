using AsitLib.CommandLine;
using System.Reflection;

namespace STOLON
{
    public sealed class KernelCommandInfoFactory : ICommandInfoFactory
    {
        public CommandInfo? Convert(CommandProvider provider, MethodInfo methodInfo, CommandAttribute attribute)
        {
            KernelCommandAttribute commandAttribute = (KernelCommandAttribute)attribute;

            CommandInfo defaultResult = MethodCommandInfo.FromMethod(methodInfo, provider);

            return new KernelCommandInfo(defaultResult.RawIds.ToArray(), defaultResult.Description, methodInfo, defaultResult.IsGenericFlag)
            {
                IsExternal = commandAttribute.IsExternal,
                RequiredFlag = commandAttribute.RequiredFlag,
                PassingPolicies = commandAttribute.PassingPolicies,
                Target = provider,
                Provider = provider,
            };
        }
    }
}
