using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON.CLI.Command
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandAttribute : Attribute
    {
        public string? IdOverride { get; }
        public string[]? Aliases { get; }
        public string Description { get; }
        public bool InheritNamespace { get; }
        public bool IsReadOnly { get; }
        public bool NeedsDev { get; }
        public CommandAttribute(
            string description,
            string? idOverride = null,
            string[]? aliases = null,
            bool inheritNamespace = true,
            bool isReadOnly = false,
            bool needsDev = false)
        {
            IdOverride = idOverride;
            Description = description;
            Aliases = aliases;
            InheritNamespace = inheritNamespace;
            IsReadOnly = isReadOnly;
            NeedsDev = needsDev;
        }
    }
}
