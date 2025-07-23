using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class MailchimpSyncService : IMailchimpSyncService
{
    private readonly NotizapDbContext _context;
    private readonly IMailchimpServiceFactory _factory;
    private readonly ILogger<MailchimpSyncService> _logger;

    public MailchimpSyncService(
        NotizapDbContext context, 
        IMailchimpServiceFactory factory, 
        ILogger<MailchimpSyncService> logger)
    {
        _context = context;
        _factory = factory;
        _logger = logger;
    }

    public async Task<MailchimpSyncResultDto> SyncAsync(string cuenta)
    {
        _logger.LogInformation("🔄 Iniciando sincronización de Mailchimp para cuenta {Cuenta}", cuenta);

        try
        {
            // Crear servicio para la cuenta específica
            var service = _factory.Create(cuenta);
            
            // Obtener campañas desde la API de Mailchimp
            _logger.LogInformation("📡 Obteniendo campañas disponibles desde API de Mailchimp para cuenta {Cuenta}", cuenta);
            var campaigns = await service.GetAvailableCampaignsAsync();
            
            _logger.LogInformation("📧 Se obtuvieron {CantidadCampanas} campañas desde la API de Mailchimp para cuenta {Cuenta}", 
                campaigns.Count, cuenta);

            if (!campaigns.Any())
            {
                _logger.LogWarning("⚠️ No se encontraron campañas para sincronizar en cuenta {Cuenta}", cuenta);
                return new MailchimpSyncResultDto
                {
                    NuevasCampañas = 0,
                    CampañasActualizadas = 0,
                    TotalProcesadas = 0,
                    Mensaje = "No se encontraron campañas para sincronizar."
                };
            }

            int nuevas = 0;
            int actualizadas = 0;
            var errores = new List<string>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                foreach (var campaign in campaigns)
                {
                    try
                    {
                        _logger.LogDebug("🔍 Procesando campaña {CampaignId} - {CampaignTitle}", 
                            campaign.Id, campaign.Title);

                        // Buscar si la campaña ya existe en la DB
                        var existingCampaign = await _context.CampaignMailchimps
                            .FirstOrDefaultAsync(c => c.CampaignId == campaign.Id && c.Cuenta == cuenta);

                        // Obtener las métricas actualizadas desde la API
                        var stats = await service.GetCampaignStatsAsync(campaign.Id);

                        if (existingCampaign == null)
                        {
                            // Campaña nueva - agregar
                            var nuevaCampaign = new CampaignMailchimp
                            {
                                CampaignId = campaign.Id,
                                Title = stats.CampaignTitle,
                                SendTime = stats.SendTime.ToUniversalTime(),
                                EmailsSent = stats.EmailsSent,
                                OpenRate = stats.OpenRate,
                                ClickRate = stats.ClickRate,
                                Conversions = stats.Conversions,
                                Cuenta = cuenta
                            };

                            _context.CampaignMailchimps.Add(nuevaCampaign);
                            nuevas++;

                            _logger.LogInformation("➕ Nueva campaña agregada: {CampaignId} - {Title} para cuenta {Cuenta}",
                                campaign.Id, stats.CampaignTitle, cuenta);
                        }
                        else
                        {
                            // Campaña existente - actualizar métricas
                            var tituloAnterior = existingCampaign.Title;
                            var emailsAnterior = existingCampaign.EmailsSent;
                            var openRateAnterior = existingCampaign.OpenRate;

                            existingCampaign.Title = stats.CampaignTitle;
                            existingCampaign.SendTime = stats.SendTime.ToUniversalTime();
                            existingCampaign.EmailsSent = stats.EmailsSent;
                            existingCampaign.OpenRate = stats.OpenRate;
                            existingCampaign.ClickRate = stats.ClickRate;
                            existingCampaign.Conversions = stats.Conversions;
                            
                            _context.CampaignMailchimps.Update(existingCampaign);
                            actualizadas++;

                            // Log detallado solo si hay cambios significativos
                            if (emailsAnterior != stats.EmailsSent || Math.Abs(openRateAnterior - stats.OpenRate) > 0.01m)
                            {
                                _logger.LogInformation("🔄 Campaña actualizada: {CampaignId} - Emails: {EmailsAntes}→{EmailsAhora}, OpenRate: {OpenRateAntes:F2}%→{OpenRateAhora:F2}%",
                                    campaign.Id, emailsAnterior, stats.EmailsSent, openRateAnterior, stats.OpenRate);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var error = $"Error procesando campaña {campaign.Id}: {ex.Message}";
                        errores.Add(error);
                        _logger.LogError(ex, "❌ Error procesando campaña individual {CampaignId} para cuenta {Cuenta}", 
                            campaign.Id, cuenta);
                    }
                }

                // Commit de la transacción
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("✅ Sincronización completada exitosamente para cuenta {Cuenta}: {Nuevas} nuevas, {Actualizadas} actualizadas",
                    cuenta, nuevas, actualizadas);

                var resultado = new MailchimpSyncResultDto
                {
                    NuevasCampañas = nuevas,
                    CampañasActualizadas = actualizadas,
                    TotalProcesadas = campaigns.Count,
                    Mensaje = GenerarMensaje(nuevas, actualizadas)
                };

                // Log de warning si hubo errores individuales pero la sync continuó
                if (errores.Any())
                {
                    _logger.LogWarning("⚠️ Sincronización completada con {CantidadErrores} errores individuales para cuenta {Cuenta}: {Errores}",
                        errores.Count, cuenta, string.Join("; ", errores));
                }

                return resultado;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "❌ Error durante transacción de sincronización para cuenta {Cuenta}, rollback ejecutado", cuenta);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error crítico durante sincronización de Mailchimp para cuenta {Cuenta}", cuenta);
            throw new Exception($"Error crítico durante sincronización de Mailchimp: {ex.Message}", ex);
        }
    }

    public async Task<List<CampaignMailchimp>> GetAllAsync(string cuenta)
    {
        _logger.LogInformation("📋 Obteniendo todas las campañas almacenadas para cuenta {Cuenta}", cuenta);

        try
        {
            var campaigns = await _context.CampaignMailchimps
                .Where(c => c.Cuenta == cuenta)
                .OrderByDescending(c => c.SendTime)
                .ToListAsync();

            _logger.LogInformation("📊 Se obtuvieron {CantidadCampanas} campañas almacenadas para cuenta {Cuenta}", 
                campaigns.Count, cuenta);

            return campaigns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener campañas almacenadas para cuenta {Cuenta}", cuenta);
            throw;
        }
    }

    private static string GenerarMensaje(int nuevas, int actualizadas)
    {
        if (nuevas == 0 && actualizadas == 0)
            return "No se encontraron cambios para sincronizar.";

        var partes = new List<string>();
        
        if (nuevas > 0)
            partes.Add($"{nuevas} campaña{(nuevas > 1 ? "s" : "")} nueva{(nuevas > 1 ? "s" : "")} agregada{(nuevas > 1 ? "s" : "")}");
        
        if (actualizadas > 0)
            partes.Add($"{actualizadas} campaña{(actualizadas > 1 ? "s" : "")} actualizada{(actualizadas > 1 ? "s" : "")}");

        return string.Join(" y ", partes) + ".";
    }
}