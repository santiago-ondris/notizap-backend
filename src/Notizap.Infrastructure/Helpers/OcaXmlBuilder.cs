using System.Text;
using System.Globalization;

public static class OcaXmlBuilder
{
    public static string GenerarXmlEnvio(Cambio cambio, string numeroCuenta)
    {
        var sb = new StringBuilder();

        string fechaOca = cambio.FechaRetiro.ToString("yyyyMMdd");

        sb.AppendLine(@"<?xml version=""1.0"" encoding=""iso-8859-1"" standalone=""yes""?>");
        sb.AppendLine(@"<ROWS>");
        sb.AppendLine($@"  <cabecera ver=""2.0"" nrocuenta=""{numeroCuenta}"" />");
        sb.AppendLine(@"  <origenes>");
        sb.AppendLine($@"    <origen calle=""{cambio.Calle}"" nro=""{cambio.Numero}"" piso=""{cambio.Piso}"" depto=""{cambio.Departamento}"" cp=""{cambio.CodigoPostal}""");
        sb.AppendLine($@"      localidad=""{cambio.Localidad}"" provincia=""{cambio.Provincia}"" contacto=""""");
        sb.AppendLine($@"      email=""{cambio.Email}"" solicitante="""" observaciones=""{cambio.Observaciones}""");
        sb.AppendLine($@"      centrocosto=""0"" idfranjahoraria=""{cambio.IdFranjaHoraria}"" idcentroimposicionorigen=""{cambio.IdCentroImposicionOrigen}"" fecha=""{fechaOca}"">");

        sb.AppendLine(@"      <envios>");
        sb.AppendLine($@"        <envio idoperativa=""{cambio.IdOperativa}"" nroremito=""{cambio.NumeroRemito}"">");

        sb.AppendLine($@"          <destinatario apellido=""{cambio.Apellido}"" nombre=""{cambio.Nombre}"" calle=""{cambio.Calle}"" nro=""{cambio.Numero}""");
        sb.AppendLine($@"            piso=""{cambio.Piso}"" depto=""{cambio.Departamento}"" localidad=""{cambio.Localidad}"" provincia=""{cambio.Provincia}""");
        sb.AppendLine($@"            cp=""{cambio.CodigoPostal}"" telefono=""{cambio.Celular}"" email=""{cambio.Email}"" idci=""{cambio.IdCentroImposicionDestino = 40}""");
        sb.AppendLine($@"            celular=""{cambio.Celular}"" observaciones=""{cambio.Observaciones}"" />");

        sb.AppendLine(@"          <paquetes>");
        sb.AppendLine($@"            <paquete alto=""{cambio.Alto.ToString("F2", CultureInfo.InvariantCulture)}"" ancho=""{cambio.Ancho.ToString("F2", CultureInfo.InvariantCulture)}""");
        sb.AppendLine($@"              largo=""{cambio.Largo.ToString("F2", CultureInfo.InvariantCulture)}"" peso=""{cambio.Peso.ToString("F2", CultureInfo.InvariantCulture)}"" valor=""{cambio.ValorDeclarado.ToString("F2", CultureInfo.InvariantCulture)}"" cant=""{cambio.Cantidad}"" />");
        sb.AppendLine(@"          </paquetes>");

        sb.AppendLine(@"        </envio>");
        sb.AppendLine(@"      </envios>");
        sb.AppendLine(@"    </origen>");
        sb.AppendLine(@"  </origenes>");
        sb.AppendLine(@"</ROWS>");

        string CleanForOca(string input)
        {
            return input
                .Replace("ñ", "n")
                .Replace("Ñ", "N")
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("Á", "A")
                .Replace("É", "E")
                .Replace("Í", "I")
                .Replace("Ó", "O")
                .Replace("Ú", "U")
                .Replace("“", "\"")
                .Replace("”", "\"")
                .Replace("’", "'")
                .Replace("‘", "'")
                .Replace("&", "y");
        }

        return CleanForOca(sb.ToString());
    }
}
