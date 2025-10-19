using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI
{
    public abstract class CommandProvider
    {
        public string Namespace { get; }
        public string FullNamespace { get; internal set; }
        public bool IsNested => FullNamespace != Namespace;
        public CommandProvider(string @namespace)
        {
            Namespace = @namespace;
            FullNamespace = @namespace;
        }
        public override string ToString() => $"CommandProvider(Id: {Namespace})";
    }
}
