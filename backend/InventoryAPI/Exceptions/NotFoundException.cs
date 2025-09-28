using System;

namespace InventoryAPI.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested entity is not found in the system.
    /// Used to translate to a 404 Not Found response in controllers.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException() 
        { 
        }

        public NotFoundException(string message) 
            : base(message) 
        { 
        }

        public NotFoundException(string message, Exception innerException) 
            : base(message, innerException) 
        { 
        }
    }
}
