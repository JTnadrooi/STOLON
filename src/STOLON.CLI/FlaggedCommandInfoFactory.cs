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

            List<string> ids = new List<string>(defaultResult.Ids);

            if (flaggedCommandAttribute.IsGenericFlag)
            {
                ids.AddRange(ids.Select(id => ParseHelpers.GetGenericFlagSignature(id)).ToArray());
            }

            return new FlaggedCommandInfo(ids.ToArray(), defaultResult.Description, methodInfo)
            {
                Flags = flaggedCommandAttribute.Flags,
                Target = provider,
                Provider = provider,
            };
        }
    }
}
