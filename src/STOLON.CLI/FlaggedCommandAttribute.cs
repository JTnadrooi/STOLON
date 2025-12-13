using AsitLib.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    [Flags]
    public enum CommandFlags
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
    public sealed class FlaggedCommandAttribute : CommandAttribute
    {
        public CommandFlags Flags { get; init; }

        public FlaggedCommandAttribute(string desc) : base(desc) { }
    }
}
