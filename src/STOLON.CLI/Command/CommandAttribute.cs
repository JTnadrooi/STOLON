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
        ReadOnly = 1,
        DevOnly = 2,
    }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CommandAttribute : Attribute
    {
        public string? IdOverride { get; }
        public string[]? Aliases { get; }
        public string Description { get; }
        public bool InheritNamespace { get; }
        public CommandFlag Flags { get; }
        public CommandAttribute(
            string description,
            string? idOverride = null,
            string[]? aliases = null,
            bool inheritNamespace = true,
            CommandFlag flags = CommandFlag.None)
        {
            IdOverride = idOverride;
            Description = description;
            Aliases = aliases;
            InheritNamespace = inheritNamespace;
            Flags = flags;
        }
    }
}
