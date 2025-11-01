using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    [Flags]
    public enum CommandFlag : int
    {
        None = 0,
        /// <summary>
        /// Specifies that the command only writes something to the screen.
        /// </summary>
        ReadOnly = 1,
        /// <summary>
        /// Specifies that the command can only be executed if <see cref="CLI.IsDev"/> is <see langword="true"/>.
        /// </summary>
        DevOnly = 2,
    }
    /// <summary>
    /// Specifies that this method can be called as a cli command. This attribute only has an effect if the method is a <see cref="CommandProvider"/> member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the <see cref="CommandInfo.Id"/>. If <see langword="null"/>, the <see cref="CommandInfo.Id"/> will be the name of the method converted to lowercase using <see cref="string.ToLower()"/>.
        /// </summary>
        public string? Id { get; }
        /// <summary>
        /// Gets the command aliases. Any <see cref="string"/> objects here will be provided as an alternative <see cref="CommandInfo.Id"/> in command calls. See <see cref="CommandInfo.Ids"/>.
        /// </summary>
        public string[]? Aliases { get; }
        /// <summary>
        /// Gets the command description. Will be displayed in the <see cref="CLICommandProvider.Help(string?)"/> command.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets if the <see cref="CommandProvider.FullNamespace"/> should be prefixed to the <see cref="CommandInfo.Id"/>, separated by a dash.
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
