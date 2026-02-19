using AsitLib.CommandLine;

namespace STOLON
{
    [Flags]
    public enum CommandFlags
    {
        None = 0,
        External = 1,
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class KernelCommandAttribute : CommandAttribute
    {
        public CommandFlags Flags { get; init; }

        public KernelCommandAttribute(string desc) : base(desc) { }
    }
}
