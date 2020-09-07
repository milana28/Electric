using System;

namespace Electric.Exceptions
{
    public class EnclosureSpecsNotFoundException : Exception
    {
        public EnclosureSpecsNotFoundException() { }
        public EnclosureSpecsNotFoundException(string message) 
            : base(message) { }
        public EnclosureSpecsNotFoundException(string message, Exception inner)
            : base(message, inner) { }
    }
}