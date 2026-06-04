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

        public bool DebugOnly { get; init; }

        public KernelCommandInfo(string[] ids, string description, MethodInfo methodInfo) : base(ids, description, methodInfo)
        {

        }

        public override string GetHelpString()
        {
            StringBuilder sb = new StringBuilder(Id).Append(" ");
            OptionInfo[] options = GetOptions();

            if (HasAliases) sb.Append($"[{Ids.Skip(1).ToJoinedString(", ")}] ");
            sb.Append($"# {Description}");

            if (options.Length != 0)
            {
                string optionsString = GetOptions().Select(p =>
                    $"\n|   {p.OptionType.Name.ToLower()}:{p.Id!.ToLower()}{(p.HasDefaultValue ? $"(def:{p.DefaultValue?.ToString() ?? StringHelpers.NULL_STRING}) " : " ")}")
                    .ToJoinedString();

                sb.Append(optionsString);
            }


            return sb.ToString() + "\n|";

            //return "a\na\na\na\na";
            //return "a\na\n. a\n. a\na";
        }
    }
}
