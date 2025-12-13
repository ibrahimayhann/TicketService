using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketApi.Data;
using TicketApi.Exceptions;
using TicketApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// CONTROLLERS + JSON AYARLARI + FILTER
// ------------------------------------------------------------
// AddControllers sadece 1 kez çaðrýlýr.
// - JsonStringEnumConverter: Enum deðerlerini sayýsal deðil string olarak döndürür (örn: "Open").
// - ModelStateValidationFilter: Model doðrulama hatalarýnda standart 400 yerine kendi exception formatýný üretmek için kullanýlýr.
builder.Services.AddControllers(options =>
{
    // Burada global filter olarak ekliyoruz -> her action'a otomatik uygulanýr.
    options.Filters.AddService<ModelStateValidationFilter>();
})
.AddJsonOptions(o =>
{
    // Enum'lar JSON çýktýsýnda string olarak görünsün diye converter ekleniyor.
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ------------------------------------------------------------
// FLUENTVALIDATION
// ------------------------------------------------------------
// AutoValidation: FluentValidation kurallarýný otomatik çalýþtýrýr (Model binding sonrasý).
builder.Services.AddFluentValidationAutoValidation();

// Bu assembly içindeki validator'larý otomatik keþfeder ve DI'a ekler.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ------------------------------------------------------------
// MODELSTATE DAVRANIÞI
// ------------------------------------------------------------
// Varsayýlan davranýþ: ModelState invalid ise ASP.NET otomatik 400 döner.
// Biz bunu kapatýyoruz; çünkü hatalarý kendi Exception formatýmýzla döneceðiz.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// ------------------------------------------------------------
// FILTER DI
// ------------------------------------------------------------
// ModelState invalid durumunu yakalayýp RequestValidationException fýrlatacak filter DI'a ekleniyor.
builder.Services.AddScoped<ModelStateValidationFilter>();

// ------------------------------------------------------------
// SWAGGER
// ------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------------------------------
// DB + DI
// ------------------------------------------------------------
// AppDbContext: SQL Server connection string appsettings.json içindeki DefaultConnection'dan okunur.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ticket servis implementasyonu DI'a ekleniyor.
builder.Services.AddScoped<ITicketService, TicketService>();

// ------------------------------------------------------------
// GLOBAL EXCEPTION MIDDLEWARE DI
// ------------------------------------------------------------
// Middleware'i UseMiddleware ile çalýþtýracaðýmýz için DI'a ekliyoruz.
builder.Services.AddScoped<ExceptionMiddleware>();

var app = builder.Build();

// ------------------------------------------------------------
// DEV ORTAMINDA SWAGGER
// ------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ------------------------------------------------------------
// GLOBAL EXCEPTION MIDDLEWARE
// ------------------------------------------------------------
// Önemli: Pipeline'da mümkün olduðunca erken çalýþmalý ki controller dahil hatalarý yakalasýn.
app.UseMiddleware<ExceptionMiddleware>();

// Eðer ileride JWT/Authentication ekleyeceksen:
// app.UseAuthentication();  // <-- Authentication varsa Authorization'dan önce gelmeli.
app.UseAuthorization();

app.MapControllers();

app.Run();
