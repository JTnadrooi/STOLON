using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace STOLON.CLI
{
    /// <summary>
    /// Represents a command provider.
    /// </summary>
    public abstract class CommandProvider
    {
        public string Namespace { get; }
        /// <summary>
        /// Gets the full namespace used in id prefixing. This may equal the <see cref="Namespace"/> but it this class is nested in a <see cref="CommandProvider"/>, the parent <see cref="CommandProvider.FullNamespace"/> gets prefixed to this.
        /// </summary>
        public string FullNamespace { get; internal set; }
        /// <summary>
        /// Gets a value indicating if this <see cref="CommandProvider"/> is nested in another <see cref="CommandProvider"/>.
        /// </summary>
        public bool IsNested => FullNamespace != Namespace;
        public CommandProvider(string @namespace)
        {
            Namespace = @namespace;
            FullNamespace = @namespace;
        }
        public override string ToString() => $"{{Namespace: {FullNamespace}, IsNested: {IsNested}}}";
    }
}
