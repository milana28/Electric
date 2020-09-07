using System;

namespace Electric.Exceptions
{
    public class EnclosureNotFoundException : Exception
    {
        public EnclosureNotFoundException() { }
        public EnclosureNotFoundException(string message) 
            : base(message) { }
        public EnclosureNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }
}