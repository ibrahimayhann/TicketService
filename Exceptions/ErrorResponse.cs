namespace TicketApi.Exceptions
{
    // ErrorResponse:
    // - Client'a dönen standart hata cevabı modelidir.
    // - ExceptionMiddleware burada bu modeli doldurup JSON olarak döndürür.
    //
    // Alanların anlamı:
    // Title  : kısa başlık ("Validation error", "Resource not found" gibi)
    // Status : HTTP status code (400, 404, 500 ...)
    // Detail : daha detaylı açıklama (genelde exception message)
    // TraceId: loglarda isteği bulmaya yarayan id (debug için çok değerli)
    // Errors : validation hatalarında alan bazlı hata listeleri
    public sealed class ErrorResponse
    {
        // init:
        // - Bu property'ler sadece object initializer ile set edilsin (immutable yaklaşım).
        public string Title { get; init; } = "An error occurred";
        public int Status { get; init; }
        public string? Detail { get; init; }

        // default!:
        // - null olacağını biliyoruz ama runtime'da mutlaka set edeceğiz demek.
        // - Middleware payload oluştururken TraceId set ediyor.
        public string TraceId { get; init; } = default!;

        // Validation hataları için:
        // "Title": ["Title zorunlu", "Max 150 karakter"] gibi
        public Dictionary<string, string[]>? Errors { get; init; }
    }
}
