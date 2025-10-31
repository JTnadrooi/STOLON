using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public class CommandInfo
    {
        public HashSet<string> Ids { get; }
        public bool HasAliases => Ids.Count > 1;
        public string Id { get; }
        public string Description { get; }
        public MethodInfo MethodInfo { get; }
        public CommandProvider Provider { get; }
        public bool IsMain { get; }
        public CommandFlag Flags { get; }

        public CommandInfo(string[] ids, CommandAttribute attribute, MethodInfo methodInfo, CommandProvider source)
        {
            Ids = ids.ToHashSet();
            Id = ids[0];
            Description = attribute.Description;
            MethodInfo = methodInfo;
            Provider = source;
            IsMain = methodInfo.Name == "_M";
            Flags = attribute.Flags;
        }

        public bool HasFlag(CommandFlag flag) => Flags.HasFlag(flag);

        public override string ToString() => $"CommandInfo(Ids: {string.Join(", ", Ids)}, Method: {MethodInfo}, Source: {Provider?.ToString()})";
    }
}
