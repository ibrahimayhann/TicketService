using System;

namespace TicketApi.Exceptions
{
    // ApiException:
    // - Uygulamada "kontrollü" olarak fırlattığımız tüm hataların temel sınıfı.
    // - Amaç: Her exception için HTTP status code ve standart bir Title taşıyabilmek.
    //
    // Neden abstract?
    // - Çünkü her hata türünün StatusCode'u farklı (404, 400, 409, 401...)
    // - Alt sınıflar StatusCode'u zorunlu override eder.
    public abstract class ApiException : Exception
    {
        // message: Exception.Message olarak taşınır.
        // Middleware bunu "detail" veya "message" olarak client'a dönebilir.
        protected ApiException(string message) : base(message) { }

        // StatusCode:
        // - Her özel exception kendi HTTP kodunu belirler.
        public abstract int StatusCode { get; }

        // Title:
        // - Hata başlığı (kısa, özet açıklama)
        // - Alt sınıflar override edebilir, default "Request failed".
        public virtual string Title => "Request failed";
    }
}
