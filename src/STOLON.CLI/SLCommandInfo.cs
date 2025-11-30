using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    /// <summary>
    /// Represents info for a specific command. See <see cref="CommandAttribute"/>.
    /// </summary>
    public sealed class SLCommandInfo : ProviderCommandInfo
    {
        public SLCommandInfo(string[] ids, SLCommandAttribute attribute, MethodInfo methodInfo, CommandProvider provider) : base(ids, attribute, methodInfo, provider)
        {
            Flags = attribute.Flags;
        }

        public CommandFlags Flags { get; }

        public bool HasFlag(CommandFlags flag) => Flags.HasFlag(flag);

        public override string ToString() => $"{{Ids: {string.Join(", ", Ids)}, Method: {MethodInfo}, Provider: {Provider?.ToString()}}}";
    }
}
