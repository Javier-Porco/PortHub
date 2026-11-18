using Microsoft.EntityFrameworkCore;
using PortHub.API.Context;
using PortHub.API.Middlewares;
using PortHub.API.Services;
using Microsoft.OpenApi;
using PortHub.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using PortHub.API.DTOs;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con SQLite
builder.Services.AddDbContext<PortHubDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
    {
        // Que ASP.NET no arme sus propios errores (ProblemDetails); los armamos nosotros.
        options.SuppressMapClientErrors = true;

        // Errores de validación de los DTOs ([Required], JSON mal formado, etc.)
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState
                .Where(e => e.Value!.Errors.Count > 0)
                .Select(e => $"{e.Key}: {string.Join(" ", e.Value!.Errors.Select(x => x.ErrorMessage))}");

            return new BadRequestObjectResult(new ErrorResponseDto
            {
                Code = "VALIDATION_ERROR",
                Message = "Los datos enviados no son válidos.",
                Details = string.Join(" | ", errores)
            });
        };
    });

// Cliente HTTP para llamar a AeroLink: URL base y API Key salen del .env, y espera como máximo 5 segundos.
builder.Services.AddHttpClient<IAeroLinkService, AeroLinkService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AeroLinkApi:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("ApiKey", builder.Configuration["AeroLinkApi:ApiKey"]);
});
builder.Services.AddScoped<ISlotService, SlotDbService>();
builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger con el botón Authorize
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Description = "Ingresá la clave. Ejemplo: ApiKey PortHubSecretKey2026",
        Name = "Authorization",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
    });
});

builder.Services.AddScoped<IEmbarqueService, EmbarqueDbService>();
builder.Services.AddHttpClient<IAeroLinkService, AeroLinkService>();

var app = builder.Build();

// Middlewares globales
app.UseMiddleware<ExceptionHandlingMiddleware>();
// Cualquier error que salga sin cuerpo (404, 405, 415...) se completa con nuestro formato.
app.UseStatusCodePages(async statusContext =>
{
    var request = statusContext.HttpContext.Request;
    var response = statusContext.HttpContext.Response;

    var error = response.StatusCode switch
    {
        404 => new ErrorResponseDto { Code = "NOT_FOUND", Message = "El recurso solicitado no existe.", Details = $"{request.Method} {request.Path}" },
        405 => new ErrorResponseDto { Code = "METHOD_NOT_ALLOWED", Message = "Método HTTP no permitido para esta ruta.", Details = $"{request.Method} {request.Path}" },
        415 => new ErrorResponseDto { Code = "UNSUPPORTED_MEDIA_TYPE", Message = "El cuerpo debe enviarse como JSON.", Details = "Use el header Content-Type: application/json." },
        _ => new ErrorResponseDto { Code = $"HTTP_{response.StatusCode}", Message = "Error en la solicitud.", Details = $"{request.Method} {request.Path}" }
    };

    await response.WriteAsJsonAsync(error);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();