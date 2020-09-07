using System;

namespace Electric.Exceptions
{
    public class ProjectNotFountException : Exception
    {
        public ProjectNotFountException() { }
        public ProjectNotFountException(string message) 
            : base(message) { }
        public ProjectNotFountException(string message, Exception inner)
            : base(message, inner) { }
    }
}