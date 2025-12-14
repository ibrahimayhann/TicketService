using Microsoft.AspNetCore.Mvc.Filters;

namespace TicketApi.Exceptions
{
    // ModelStateValidationFilter:
    
    public sealed class ModelStateValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // ModelState invalid ise:
            // - Hangi alanlarda hata var, tek tek topla
            if (!context.ModelState.IsValid)
            {
               
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

       
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
