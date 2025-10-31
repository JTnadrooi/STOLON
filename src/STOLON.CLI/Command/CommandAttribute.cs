using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Command
{
    [Flags]
    public enum CommandFlag : int
    {
        None = 0,
        /// <summary>
        /// The command only writes something to the screen.
        /// </summary>
        ReadOnly = 1,
        /// <summary>
        /// The command can only be executed if <see cref="CLI.IsDev"/> is <see langword="true"/>.
        /// </summary>
        DevOnly = 2,
    }
    /// <summary>
    /// Specifies that this method can be called from a cli command. This attribute only has an effect if the method is a <see cref="CommandProvider"/> member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        /// The command id. If <see langword="null"/>, the <see cref="CommandInfo.Id"/> will be the name of the method converted to lowercase using <see cref="string.ToLower()"/>.
        /// </summary>
        public string? Id { get; }
        public string[]? Aliases { get; }
        public string Description { get; }
        /// <summary>
        /// If <see cref="true"/>, the <see cref="CommandProvider.FullNamespace"/> <i>(+ "-")</i> will be prefixed to the <see cref="CommandInfo.Id"/>.
        /// </summary>
        public bool InheritNamespace { get; }
        public CommandFlag Flags { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class by using a specified command parameters.
        /// </summary>
        /// <param name="description"><inheritdoc cref="Description" path="/summary"/></param>
        /// <param name="id"><inheritdoc cref="Id" path="/summary"/></param>
        /// <param name="aliases"><inheritdoc cref="Aliases" path="/summary"/></param>
        /// <param name="inheritNamespace"><inheritdoc cref="InheritNamespace" path="/summary"/></param>
        /// <param name="flags"><inheritdoc cref="Flags" path="/summary"/></param>
        public CommandAttribute(
            string description,
            string? id = null,
            string[]? aliases = null,
            bool inheritNamespace = true,
            CommandFlag flags = CommandFlag.None)
        {
            Id = id;
            Description = description;
            Aliases = aliases;
            InheritNamespace = inheritNamespace;
            Flags = flags;
        }
    }
}
