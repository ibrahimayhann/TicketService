using Microsoft.AspNetCore.Http;

namespace TicketApi.Exceptions
{
   
    public sealed class NotFoundException : ApiException
    {
        public NotFoundException(string message) : base(message) { }

        // 404
        public override int StatusCode => StatusCodes.Status404NotFound;

        
        public override string Title => "Resource not found";
    }
}
