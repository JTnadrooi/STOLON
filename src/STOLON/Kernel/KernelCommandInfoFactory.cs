using AsitLib.CommandLine;
using System.ComponentModel.Design;
using System.Reflection;

namespace STOLON
{
    public sealed class KernelCommandInfoFactory : ICommandInfoFactory
    {
        public CommandInfo? Convert(CommandProvider provider, MethodInfo methodInfo, CommandAttribute attribute)
        {
            KernelCommandAttribute commandAttribute = (KernelCommandAttribute)attribute;

            CommandInfo defaultResult = MethodCommandInfo.FromMethod(methodInfo, provider);

            List<string> ids = new List<string>(defaultResult.Ids);

            if (commandAttribute.IsExternal)
            {
                ids = ids.Select(id => "%" + id).ToList(); // to list so linq gets executed first.
            }

            if (commandAttribute.IsGenericFlag)
            {
                ids.AddRange(ids.Select(id => ParseHelpers.GetGenericFlagSignature(id)).ToArray());
            }

            return new KernelCommandInfo(ids.ToArray(), defaultResult.Description, methodInfo)
            {
                RequiredFlag = commandAttribute.RequiredFlag,
                Target = provider,
                Provider = provider,
            };
        }
    }
}
