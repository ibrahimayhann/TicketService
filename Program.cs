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
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ModelStateValidationFilter>();
})
.AddJsonOptions(o =>
{
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ------------------------------------------------------------
// FLUENTVALIDATION
// ------------------------------------------------------------
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ------------------------------------------------------------
// MODELSTATE DAVRANIŞI
// ------------------------------------------------------------
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// ------------------------------------------------------------
// FILTER DI
// ------------------------------------------------------------
builder.Services.AddScoped<ModelStateValidationFilter>();

// ------------------------------------------------------------
// SWAGGER
// ------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------------------------------
// DB + DI
// ------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITicketService, TicketService>();

// ------------------------------------------------------------
// GLOBAL EXCEPTION MIDDLEWARE DI
// ------------------------------------------------------------
builder.Services.AddScoped<ExceptionMiddleware>();

// ------------------------------------------------------------
// CORS (Vite Frontend: http://localhost:5173)
// ------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("ViteCors", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

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

// ✅ CORS middleware (Controller'lara gitmeden önce çalışmalı)
app.UseCors("ViteCors");

// ------------------------------------------------------------
// GLOBAL EXCEPTION MIDDLEWARE
// ------------------------------------------------------------
app.UseMiddleware<ExceptionMiddleware>();

// Eğer ileride JWT/Authentication ekleyeceksen:
// app.UseAuthentication();  // <-- Authentication varsa Authorization'dan önce gelmeli.
app.UseAuthorization();

app.MapControllers();

app.Run();
