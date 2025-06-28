public class MailchimpSyncResultDto
{
    public int NuevasCampañas { get; set; }
    public int CampañasActualizadas { get; set; }
    public int TotalProcesadas { get; set; }
    public string Mensaje { get; set; } = default!;
}