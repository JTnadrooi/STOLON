using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public class InvalidResourceException : Exception
    {
        public InvalidResourceException() : base() { }
        public InvalidResourceException(string? message) : base(message) { }
        public InvalidResourceException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
