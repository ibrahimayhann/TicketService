namespace TicketApi.Exceptions
{
    public sealed class RequestValidationException:ApiException
    {
        public RequestValidationException(Dictionary<string, string[]> errors)
        : base("Validation failed")
        {
            Errors = errors;
        }

        public override int StatusCode => StatusCodes.Status400BadRequest;
        public override string Title => "Validation error";
        public Dictionary<string, string[]> Errors { get; }
    }
}
