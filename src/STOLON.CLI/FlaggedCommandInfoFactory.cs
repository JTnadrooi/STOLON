using AsitLib.CommandLine;
using System.Reflection;

namespace STOLON.CLI
{
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
}
