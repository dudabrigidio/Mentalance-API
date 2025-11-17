using Mentalance.Connection;
using Mentalance.Converters;
using Mentalance.ML.Service;
using Mentalance.Repository;
using Mentalance.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Serilog;
using Serilog.Events;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using System.Diagnostics;


var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)  
    .Enrich.FromLogContext() 
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

// Substituir o logger padrão pelo Serilog
builder.Host.UseSerilog(); 

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new EmocaoEnumJsonConverter());
    });

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "Mentalance.API",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.MachineName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.EnrichWithHttpRequest = (activity, request) =>
            {
                activity.SetTag("http.request.method", request.Method);
                activity.SetTag("http.request.path", request.Path);
            };
            options.EnrichWithHttpResponse = (activity, response) =>
            {
                activity.SetTag("http.response.status_code", response.StatusCode);
            };
        })
        .AddHttpClientInstrumentation()
        .AddSource("Mentalance.*")
        .AddConsoleExporter()
        );
    
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    // Configura o Swagger para usar strings nos enums
    c.UseInlineDefinitionsForEnums();
});


// Versionamento da API

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0); // versão padrão
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true; // mostra no header da resposta quais versões existem
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"), // ex: ?api-version=1.0
        new HeaderApiVersionReader("x-api-version"),    // ex: Header x-api-version: 1.0
        new UrlSegmentApiVersionReader()                // ex: /api/v1/usuarios
    );
});

// Permite que o Swagger reconheça as versões da API
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // exibe como v1, v2
    options.SubstituteApiVersionInUrl = true;
});

// Banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));


// Registra Repositórios
builder.Services.AddScoped<ICheckinRepository, CheckinRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAnaliseSemanalRepository, AnaliseSemanalRepository>();

// Registra Serviços
builder.Services.AddScoped<ICheckinService, CheckinService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAnaliseSemanalService, AnaliseSemanalService>();

// Registra MLService
builder.Services.AddScoped<IMLService, MLService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "database",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "sql", "oracle" });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:8081") // ou a URL do seu app front
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCors", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// Swagger 

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Cria uma aba do Swagger para cada versão
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                $"Mentalance API {description.GroupName.ToUpper()}");
        }
    });
}


app.UseHttpsRedirection();

app.UseCors("AllowCors");

// Configurar logging automático de requisições HTTP
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : elapsed > 1000
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    };
});

app.UseAuthorization();

app.MapControllers();

// Health Check endpoints
app.MapHealthChecks("/health");


// Configurar logging de inicialização e encerramento
try
{
    Log.Information("Iniciando aplicação Mentalance");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}


// Torna a classe Program acessível para testes
/// <summary>
/// Classe parcial do Program que torna o ponto de entrada acessível para testes de integração.
/// </summary>
public partial class Program { }
