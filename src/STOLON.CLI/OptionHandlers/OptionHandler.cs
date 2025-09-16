using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STOLON.CLI.CommandHelpers;

namespace STOLON.CLI
{
    //public struct OptionInfo
    //{
    //    public string Id { get; }
    //    public string LongId { get; }
    //    public string Description { get; }

    //    public OptionInfo(string id, string longId, string description)
    //    {
    //        Id = id;
    //        LongId = longId;
    //        Description = description;
    //    }
    //}

    public abstract class OptionHandler
    {
        public string? ShortId { get; }
        public string LongId { get; }
        public string Description { get; }

        private string? _shortIdFull;
        private string _longIdFull;

        public OptionHandler(string longId, string description, string? shortId = null)
        {
            ShortId = shortId;
            LongId = longId;
            Description = description;

            _shortIdFull = shortId != null ? ("-" + shortId) : null;
            _longIdFull = "--" + longId;
        }
        public virtual void PreCommand(ArgumentsInfo arguments) { }
        public virtual void PostCommand() { }

        public bool ShouldListen(ArgumentsInfo args) => (_shortIdFull != null ? args.Options.Contains(_shortIdFull) : false) || args.Options.Contains(_longIdFull);
    }
}
