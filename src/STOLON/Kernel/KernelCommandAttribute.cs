using AsitLib.CommandLine;

namespace STOLON
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class KernelCommandAttribute : CommandAttribute
    {
        public bool IsExternal { get; init; }

        public Type? RequiredFlag { get; init; } // verified in commandinfo.

        public KernelCommandAttribute(string desc) : base(desc) { }
    }
}
