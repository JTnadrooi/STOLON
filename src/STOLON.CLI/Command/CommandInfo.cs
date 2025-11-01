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
    public sealed class CommandInfo
    {
        /// <summary>
        /// A <see cref="HashSet{T}"/> containing all command ids. Includes <see cref="Id"/>.
        /// </summary>
        public HashSet<string> Ids { get; }
        public bool HasAliases => Ids.Count > 1;
        public string Id { get; }

        /// <summary>
        /// <inheritdoc cref="CommandAttribute.Description" path="/summary"/>
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The methodinfo this <see cref="CommandInfo"/> is create from.
        /// </summary>
        public MethodInfo MethodInfo { get; }
        public CommandProvider Provider { get; }

        /// <summary>
        /// Gets a value indicating if this command is the main command. Main commands inherit their <see cref="Id"/> from the source <see cref="CommandProvider.FullNamespace"/>.
        /// </summary>
        public bool IsMain { get; }

        /// <summary>
        /// <inheritdoc cref="CommandAttribute.Flags" path="/summary"/>
        /// </summary>
        public CommandFlag Flags { get; }

        /// <param name="ids"><inheritdoc cref="Ids" path="/summary"/></param>
        /// <param name="attribute"></param>
        /// <param name="methodInfo"><inheritdoc cref="MethodInfo" path="/summary"/></param>
        /// <param name="provider"><inheritdoc cref="Provider" path="/summary"/></param>
        public CommandInfo(string[] ids, CommandAttribute attribute, MethodInfo methodInfo, CommandProvider provider)
        {
            Ids = ids.ToHashSet();
            Id = ids[0];
            Description = attribute.Description;
            MethodInfo = methodInfo;
            Provider = provider;
            IsMain = methodInfo.Name == "_M";
            Flags = attribute.Flags;
        }

        public bool HasFlag(CommandFlag flag) => Flags.HasFlag(flag);

        public override string ToString() => $"{{Ids: {string.Join(", ", Ids)}, Method: {MethodInfo}, Provider: {Provider?.ToString()}}}";
    }
}
