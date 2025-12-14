using System;

namespace TicketApi.Exceptions
{
    
    public abstract class ApiException : Exception
    {
        
        protected ApiException(string message) : base(message) { }

        
        public abstract int StatusCode { get; }

       
        public virtual string Title => "Request failed";
    }
}
