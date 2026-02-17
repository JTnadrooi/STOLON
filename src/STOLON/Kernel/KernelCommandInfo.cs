using AsitLib.CommandLine;
using System.Reflection;

namespace STOLON
{
    public sealed class KernelCommandInfo : MethodCommandInfo
    {
        public KernelCommandInfo(string[] ids, string description, MethodInfo methodInfo)
            : base(ids, description, methodInfo)
        {

        }
    }
}
