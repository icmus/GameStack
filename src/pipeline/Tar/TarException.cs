using System;

namespace GameStack.Pipeline.Tar
{
    public class TarException : Exception
    {
        public TarException(string message) : base(message)
        {
        }
    }
}