using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STOLON.CLI.CommandHelpers;

namespace STOLON.CLI
{
    /// <summary>
    /// Represents a command flag listener and handler.
    /// </summary>
    public abstract class FlagHandler
    {
        public string? ShortId { get; }
        public string LongId { get; }
        public string Description { get; }

        public FlagHandler(string longId, string description, string? shortId = null)
        {
            ShortId = shortId;
            LongId = longId;
            Description = description;
        }

        /// <summary>
        /// Gets called before the command executes. Only calls if the <see cref="ShouldListen(ArgumentsInfo)"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="arguments">The command arguments. Do not check if this flag is present, that is done by the <see cref="ShouldListen(ArgumentsInfo)"/> method.</param>
        public virtual void PreCommand(ArgumentsInfo arguments) { }

        /// <summary>
        /// Gets called after the command executes. Only calls if the <see cref="ShouldListen(ArgumentsInfo)"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="arguments">The command arguments. Do not check if this flag is present, that is done by the <see cref="ShouldListen(ArgumentsInfo)"/> method.</param>
        public virtual void PostCommand(ArgumentsInfo arguments) { }

        /// <summary>
        /// Checks if the current <see cref="FlagHandler"/> should run its <see cref="PreCommand(ArgumentsInfo)"/> and <see cref="PostCommand"/> actions.
        /// </summary>
        /// <param name="args"></param>
        /// <returns><see langword="true"/> if </returns>
        public bool ShouldListen(ArgumentsInfo args) => (ShortId == null ? false : args.Flags.Contains(ShortId)) || args.Flags.Contains(LongId);
    }
}
