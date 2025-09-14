using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static STOLON.CLI.CommandHelpers;

namespace STOLON.CLI
{
    public struct Option
    {
        public string Id { get; }
        public string LongId { get; }
        public string Description { get; }

        public Option(string id, string longId, string description)
        {
            Id = id;
            LongId = longId;
            Description = description;
        }
    }

    public interface IOptionHandler
    {
        public void PreCommand(ArgumentsInfo arguments);
        public void PostCommand();
        public Option[] GetOptions();
    }
}
