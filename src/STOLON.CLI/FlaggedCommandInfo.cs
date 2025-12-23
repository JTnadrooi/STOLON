using AsitLib.CommandLine;
using System.Reflection;

namespace STOLON.CLI
{
    /// <summary>
    /// Represents info for a specific command. See <see cref="CommandAttribute"/>.
    /// </summary>
    public sealed class FlaggedCommandInfo : MethodCommandInfo
    {
        public CommandFlags Flags { get; init; }

        public FlaggedCommandInfo(string[] ids, string description, MethodInfo methodInfo)
            : base(ids, description, methodInfo)
        {

        }

        public bool HasFlag(CommandFlags flag) => Flags.HasFlag(flag);

        public override string ToString() => $"{{Ids: {string.Join(", ", Ids)}, Method: {MethodInfo}, Provider: {Provider?.ToString()}}}";
    }
}
