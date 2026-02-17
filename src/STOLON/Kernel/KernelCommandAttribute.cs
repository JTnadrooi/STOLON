using AsitLib.CommandLine;

namespace STOLON
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class KernelCommandAttribute : CommandAttribute
    {
        public KernelCommandAttribute(string desc) : base(desc) { }
    }
}
