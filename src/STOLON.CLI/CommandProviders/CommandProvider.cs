using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public abstract class CommandProvider
    {
        public string Id { get; }
        public CommandProvider(string id) => Id = id;
        public override string ToString() => $"CommandProvider(Id: {Id})";
    }
}
