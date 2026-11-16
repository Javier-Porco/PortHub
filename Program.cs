using Microsoft.EntityFrameworkCore;
using PortHub.API.Context;
using PortHub.API.Middlewares;
using PortHub.API.Services;
using Microsoft.OpenApi;
using PortHub.API.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configurar DbContext con SQLite
builder.Services.AddDbContext<PortHubDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddScoped<IAerolineaService, AerolineaDbService>();
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