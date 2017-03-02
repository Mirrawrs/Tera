using System;

namespace SampleBot
{
    //Just a custom exception type to differentiate module-thrown exceptions.
    internal class MyTeraException : Exception
    {
        public MyTeraException(string message) : base(message)
        {
        }
    }
}