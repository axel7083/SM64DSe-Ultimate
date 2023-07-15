using System;

namespace SM64DSe.Exceptions
{
    public class RomEditorException : Exception
    {
        public RomEditorException()
        {
        }

        public RomEditorException(string message)
            : base(message)
        {
        }

        public RomEditorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}