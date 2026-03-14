using AsitLib.CommandLine;

namespace STOLON
{
    /// <summary>
    /// Represents a command parsable to a <see cref="KernelCommandInfo"/> instance using a <see cref="KernelCommandInfoFactory"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class KernelCommandAttribute : CommandAttribute
    {
        public Type? RequiredFlag { get; init; } // verified in commandinfo.

        public bool IsExternal { get; init; }

        public bool IsGenericFlag { get; init; }

        public KernelCommandAttribute(string desc) : base(desc) { }
    }
}
