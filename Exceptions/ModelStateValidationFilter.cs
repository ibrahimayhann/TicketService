using Microsoft.AspNetCore.Mvc.Filters;

namespace TicketApi.Exceptions
{
    // ModelStateValidationFilter:
    // - Controller action çalışmadan önce ModelState'i kontrol eder.
    // - Eğer model binding / validation sonucu geçersizse,
    //   burada RequestValidationException fırlatır.
    //
    // Neden filter?
    // - [ApiController] normalde otomatik 400 döndürür.
    // - Sen Program.cs'te SuppressModelStateInvalidFilter = true yaptın.
    // - Bu sayede otomatik 400'ü kapatıp, hatayı kendi formatında döndürüyorsun.
    public sealed class ModelStateValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // ModelState invalid ise:
            // - Hangi alanlarda hata var, tek tek topla
            if (!context.ModelState.IsValid)
            {
                // ModelState -> Dictionary<string, string[]>
                // Key: alan adı (örn: "Title" veya "request.Title")
                // Value: o alanın hata mesajları
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                // Kontrolü durdurmak için exception fırlatıyoruz.
                // ExceptionMiddleware bunu yakalayıp 400 + errors ile dönecek.
                throw new RequestValidationException(errors);
            }
        }

        // Action tamamlandıktan sonra ekstra işimiz yok
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
