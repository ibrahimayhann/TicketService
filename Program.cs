using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketApi.Data;
using TicketApi.Exceptions;
using TicketApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Enum string
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// FluentValidation (auto-validation)
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ModelState invalid response'u default pipeline'a býrakma (biz exception formatýnda döneceðiz)
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Action filter ile ModelState invalid yakala -> RequestValidationException fýrlat
builder.Services.AddScoped<ModelStateValidationFilter>();

builder.Services.AddControllers(opt =>
{
    opt.Filters.AddService<ModelStateValidationFilter>();
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Db + DI
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITicketService, TicketService>();

// Global exception middleware
builder.Services.AddScoped<ExceptionMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Global exception first (controller hatalarýný yakalasýn)
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
