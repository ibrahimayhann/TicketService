using System.Text.Json;

namespace TicketApi.Exceptions;

// ExceptionMiddleware:
// Uygulamadaki tüm hataları tek noktadan yakalayıp,
// standart bir JSON formatında response döndürmek için kullanılır.
//
// IMiddleware kullanımı:
// - DI ile middleware'i injectable yapar (constructor üzerinden ILogger gibi şeyleri alabilir).
// - Program.cs'te: app.UseMiddleware<ExceptionMiddleware>(); ile pipeline'a eklenir.
public sealed class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;

    // JSON ayarlarını her istekte tekrar tekrar oluşturmamak için static tutulabilir.
    // (Küçük ama temiz bir optimizasyon)
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        // JSON alan isimleri camelCase olsun: status, title, detail, traceId, errors ...
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
    {
        // ILogger: hataları loglamak için kullanılır
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            // Pipeline'daki bir sonraki middleware / controller çalıştırılır.
            await next(context);
        }
        catch (RequestValidationException ex)
        {
            // ------------------------------------------------------------
            // VALIDATION HATASI (400 gibi)
            // ------------------------------------------------------------
            // ex.Errors genelde şöyle bir yapı taşır:
            // {
            //   "title": ["Title zorunlu", "Max 150 karakter"],
            //   "priority": ["Priority seçilmelidir"]
            // }
            //
            // Bu sayede front-end alan bazlı hataları kolayca gösterebilir.
            await Write(context, ex.StatusCode, ex.Title, ex.Message, ex.Errors);
        }
        catch (ApiException ex)
        {
            // ------------------------------------------------------------
            // BUSINESS / API HATALARI (404, 409, 401, 403 vb.)
            // ------------------------------------------------------------
            // NotFoundException gibi custom exception'ların base'i genelde ApiException olur.
            // errors yoksa null geçiyoruz.
            await Write(context, ex.StatusCode, ex.Title, ex.Message, null);
        }
        catch (Exception ex)
        {
            // ------------------------------------------------------------
            // BEKLENMEYEN HATALAR (500)
            // ------------------------------------------------------------
            // Burada gerçek exception detayını client'a vermiyoruz (güvenlik ve profesyonellik).
            // Ama log'a yazıyoruz ki debug edebilelim.
            _logger.LogError(ex, "Unhandled exception");

            await Write(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "Unexpected error occurred.",
                null
            );
        }
    }

    private static async Task Write(
        HttpContext context,
        int status,
        string title,
        string detail,
        Dictionary<string, string[]>? errors)
    {
        // ------------------------------------------------------------
        // KRİTİK KONTROL: Response başladıysa tekrar yazamazsın!
        // ------------------------------------------------------------
        // Eğer daha önce header/body yazıldıysa (ör: streaming, file, vb.)
        // burada StatusCode/ContentType set etmek veya tekrar body yazmak hata üretir.
        if (context.Response.HasStarted)
        {
            // Response başladığında buradan döneriz.
            // (Bu senaryoda en azından uygulama patlamasın.)
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        // ErrorResponse:
        // Projede standart hata formatı.
        // TraceId:
        // - Bu id sayesinde loglarda ilgili hatayı bulmak kolaylaşır.
        var payload = new ErrorResponse
        {
            Status = status,
            Title = title,
            Detail = detail,
            TraceId = context.TraceIdentifier,
            Errors = errors
        };

        // JSON SERIALIZATION GÜVENCESİ  Eğer Serialize hata verirse bile kullanıcıya basit bir JSON döndürürüz.
        string json;
        try
        {
            json = JsonSerializer.Serialize(payload, _jsonOptions);
        }
        catch
        {
            // Fallback: serialize edilemez bir durum olursa en minimal error json
            json = "{\"status\":500,\"title\":\"Internal server error\",\"detail\":\"Unexpected error occurred.\",\"traceId\":\""
                   + context.TraceIdentifier + "\"}";
        }

        await context.Response.WriteAsync(json);
    }
}
