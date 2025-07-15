using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Notizap.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous] // Para facilitar las pruebas sin autenticación
public class TestLoggingController : ControllerBase
{
    private readonly ILogger<TestLoggingController> _logger;

    public TestLoggingController(ILogger<TestLoggingController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Endpoint para probar diferentes niveles de logging
    /// </summary>
    [HttpGet("test-levels")]
    public IActionResult TestLogLevels()
    {
        // 🔍 TRACE - Información muy detallada (normalmente disabled en producción)
        _logger.LogTrace("🔍 TRACE: Este es un log de trace - información muy detallada");

        // 🐛 DEBUG - Información para debugging
        _logger.LogDebug("🐛 DEBUG: Este es un log de debug - útil para desarrollo");

        // ℹ️ INFORMATION - Información general
        _logger.LogInformation("ℹ️ INFO: TestLoggingController ejecutándose correctamente");

        // ⚠️ WARNING - Algo que podría ser un problema
        _logger.LogWarning("⚠️ WARNING: Esto es un warning de ejemplo");

        // ❌ ERROR - Error recuperable
        _logger.LogError("❌ ERROR: Esto es un error de ejemplo (pero controlado)");

        // 💥 CRITICAL - Error fatal (no se usa porque crashea)
        // _logger.LogCritical("💥 CRITICAL: Error crítico del sistema");

        return Ok(new
        {
            message = "✅ Todos los niveles de logging ejecutados correctamente",
            timestamp = DateTime.UtcNow,
            controller = nameof(TestLoggingController)
        });
    }

    /// <summary>
    /// Endpoint para probar structured logging con propiedades
    /// </summary>
    [HttpGet("test-structured")]
    public IActionResult TestStructuredLogging()
    {
        var userId = 123;
        var username = "testuser";
        var action = "TestStructuredLogging";
        var executionTime = 45.67;

        // 🎯 STRUCTURED LOGGING - Con propiedades específicas
        _logger.LogInformation(
            "Usuario {UserId} ({Username}) ejecutó acción {Action} en {ExecutionTime}ms",
            userId, username, action, executionTime);

        // Logging con objeto complejo
        var requestData = new
        {
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"].ToString(),
            RequestId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Request details: {@RequestData}",
            requestData);

        return Ok(new
        {
            message = "✅ Structured logging test completado",
            data = requestData
        });
    }

    /// <summary>
    /// Endpoint para simular un error y ver cómo se loggea
    /// </summary>
    [HttpGet("test-error")]
    public IActionResult TestErrorLogging()
    {
        try
        {
            // Simulamos una operación que puede fallar
            var randomNumber = new Random().Next(1, 10);
            
            _logger.LogInformation("🎲 Número random generado: {RandomNumber}", randomNumber);

            if (randomNumber <= 5)
            {
                throw new InvalidOperationException($"El número {randomNumber} es demasiado bajo!");
            }

            _logger.LogInformation("✅ Operación exitosa con número {RandomNumber}", randomNumber);
            
            return Ok(new
            {
                message = "Operación exitosa",
                number = randomNumber,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            // 🚨 LOGGING DE EXCEPCIONES con contexto
            _logger.LogError(ex, 
                "❌ Error en TestErrorLogging. Usuario: {UserId}, Timestamp: {Timestamp}",
                "anonymous", DateTime.UtcNow);

            return StatusCode(500, new
            {
                error = "Error interno del servidor",
                message = "Ver logs para más detalles",
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Endpoint para probar performance logging
    /// </summary>
    [HttpGet("test-performance")]
    public async Task<IActionResult> TestPerformanceLogging()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        _logger.LogInformation("🚀 Iniciando operación de performance test");

        // Simulamos trabajo asíncrono
        await Task.Delay(Random.Shared.Next(100, 1000));

        stopwatch.Stop();
        var executionTime = stopwatch.ElapsedMilliseconds;

        // Logging condicional basado en performance
        if (executionTime > 500)
        {
            _logger.LogWarning(
                "⚠️ Operación lenta detectada: {ExecutionTime}ms (umbral: 500ms)",
                executionTime);
        }
        else
        {
            _logger.LogInformation(
                "✅ Operación rápida completada en {ExecutionTime}ms",
                executionTime);
        }

        return Ok(new
        {
            message = "Performance test completado",
            executionTimeMs = executionTime,
            isSlowOperation = executionTime > 500,
            timestamp = DateTime.UtcNow
        });
    }
}