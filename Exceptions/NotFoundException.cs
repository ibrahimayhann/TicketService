using Microsoft.AspNetCore.Http;

namespace TicketApi.Exceptions
{
    // NotFoundException:
    // - İstenen kaynak bulunamadığında fırlatılır (Ticket yok, Comment yok vb.)
    // - Middleware bunu yakalayıp 404 döndürür.
    public sealed class NotFoundException : ApiException
    {
        public NotFoundException(string message) : base(message) { }

        // 404
        public override int StatusCode => StatusCodes.Status404NotFound;

        // Client'a dönecek başlık
        public override string Title => "Resource not found";
    }
}
