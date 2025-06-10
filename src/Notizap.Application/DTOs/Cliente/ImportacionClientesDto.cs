public class ImportacionClientesDto
{
    public int Id { get; set; }
    public string? NombreArchivo { get; set; }
    public DateTime FechaImportacion { get; set; }
    public int CantidadClientesNuevos { get; set; }
    public int CantidadComprasNuevas { get; set; }
}