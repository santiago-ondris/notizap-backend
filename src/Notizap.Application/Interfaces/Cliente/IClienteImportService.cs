public interface IClienteImportService
{
    Task<ImportacionClientesDto> ImportarDesdeExcelAsync(Stream excelFileStream, string nombreArchivo);
    List<string> ValidarArchivo(Stream excelFileStream);
}