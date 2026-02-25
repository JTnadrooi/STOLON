using AsitLib.CommandLine;
using System.Reflection;

namespace STOLON
{
    public sealed class KernelCommandInfo : MethodCommandInfo
    {
        private Type? _requiredFlag;
        public Type? RequiredFlag
        {
            get => _requiredFlag;
            init
            {
                if (value is null)
                {
                    _requiredFlag = null;
                    return;
                }

                if (!value.IsAssignableTo<CommandFlag>()) throw new ArgumentException("Invalid flagtype.", nameof(value));

                _requiredFlag = value;
            }
        }

        public KernelCommandInfo(string[] ids, string description, MethodInfo methodInfo, bool isGenericFlag) : base(ids, description, methodInfo, isGenericFlag)
        {

        }
    }
}
