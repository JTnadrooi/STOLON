using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STOLON.CLI.CommandHelpers;

namespace STOLON.CLI
{
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

        public virtual void PreCommand(ArgumentsInfo arguments) { }
        public virtual void PostCommand() { }

        public bool ShouldListen(ArgumentsInfo args) => (ShortId == null ? false : args.Flags.Contains(ShortId)) || args.Flags.Contains(LongId);
    }
}
