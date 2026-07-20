using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    /// <summary>
    /// The user interface for the <see cref="Environment"/>.
    /// </summary>
    [Dependency(ServiceLifetime.Singleton)]
    public static class Interface
    {
        public const int LineWidth = 1;
    }
}
