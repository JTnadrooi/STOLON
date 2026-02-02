using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public enum ServiceLifetime
    {
        Singleton,
        Scoped,
        Transient,
    }

    public class DependencyAttribute : Attribute
    {
        public ServiceLifetime Lifetime { get; }

        public DependencyAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}
