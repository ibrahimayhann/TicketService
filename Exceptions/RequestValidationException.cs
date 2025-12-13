using Microsoft.AspNetCore.Http;

namespace TicketApi.Exceptions
{
    // RequestValidationException:
    // - ModelState geçersiz olduğunda fırlatılır.
    // - Errors dictionary'si ile alan bazlı hataları taşır.
    public sealed class RequestValidationException : ApiException
    {
        public RequestValidationException(Dictionary<string, string[]> errors)
            : base("Validation failed") // Detail tarafında gösterilebilecek genel mesaj
        {
            Errors = errors;
        }

        // 400
        public override int StatusCode => StatusCodes.Status400BadRequest;

        public override string Title => "Validation error";

        // Hangi alanlarda hangi hata var?
        public Dictionary<string, string[]> Errors { get; }
    }
}
