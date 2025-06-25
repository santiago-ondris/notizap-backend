public static class PublicidadDashboardHelper
{
    public static PublicidadResumenGeneralDto CalcularResumenGeneral(
        List<AdReport> reportesActuales, 
        List<AdReport> reportesAnteriores)
    {
        // Totales actuales (todos los datos ya están en AdReports)
        var gastoTotalActual = reportesActuales.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);
        var gastoTotalAnterior = reportesAnteriores.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);

        // Métricas solo de campañas que tienen datos (las sincronizadas de Meta)
        var campañasConMetricas = reportesActuales.SelectMany(r => r.Campañas)
            .Where(c => c.Clicks > 0 || c.Impressions > 0 || c.Reach > 0);

        var totalClicks = campañasConMetricas.Sum(c => c.Clicks);
        var totalImpressions = campañasConMetricas.Sum(c => c.Impressions);
        var totalReach = campañasConMetricas.Sum(c => c.Reach);
        var ctrPromedio = totalImpressions > 0 ? (decimal)totalClicks / totalImpressions * 100 : 0;
        var costoPromedioPorClick = totalClicks > 0 ? gastoTotalActual / totalClicks : 0;

        var porcentajeCambio = gastoTotalAnterior > 0 ? 
            ((gastoTotalActual - gastoTotalAnterior) / gastoTotalAnterior) * 100 : 0;

        return new PublicidadResumenGeneralDto
        {
            GastoTotalActual = gastoTotalActual,
            GastoTotalAnterior = gastoTotalAnterior,
            PorcentajeCambio = Math.Round(porcentajeCambio, 2),
            TotalCampañasActivas = reportesActuales.SelectMany(r => r.Campañas).Count(),
            TotalClicks = totalClicks,
            TotalImpressions = totalImpressions,
            TotalReach = totalReach,
            CtrPromedio = Math.Round(ctrPromedio, 2),
            CostoPromedioPorClick = Math.Round(costoPromedioPorClick, 2)
        };
    }

    public static List<PublicidadResumenUnidadDto> CalcularResumenPorUnidad(
        List<AdReport> reportesActuales,
        List<AdReport> reportesAnteriores)
    {
        var unidades = new[] { "montella", "alenka", "kids" };
        var resultado = new List<PublicidadResumenUnidadDto>();

        var gastoTotalGeneral = reportesActuales.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);

        foreach (var unidad in unidades)
        {
            // Reportes de la unidad actual
            var reportesUnidadActual = reportesActuales
                .Where(r => r.UnidadNegocio.Equals(unidad, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var reportesUnidadAnterior = reportesAnteriores
                .Where(r => r.UnidadNegocio.Equals(unidad, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Calcular totales
            var gastoTotal = reportesUnidadActual.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);
            var gastoTotalAnterior = reportesUnidadAnterior.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);

            var cambioVsAnterior = gastoTotalAnterior > 0 ? 
                ((gastoTotal - gastoTotalAnterior) / gastoTotalAnterior) * 100 : 0;

            // Métricas de performance (solo campañas con métricas de Meta)
            var campañasConMetricas = reportesUnidadActual.SelectMany(r => r.Campañas)
                .Where(c => c.Clicks > 0 || c.Impressions > 0 || c.Reach > 0);

            var performance = 0m;
            if (gastoTotal > 0 && campañasConMetricas.Any())
            {
                var ctrPromedio = campañasConMetricas.Average(c => 
                    c.Impressions > 0 ? (decimal)c.Clicks / c.Impressions * 100 : 0);
                var reachTotal = campañasConMetricas.Sum(c => c.Reach);
                performance = (ctrPromedio * reachTotal) / gastoTotal;
            }

            // Followers obtenidos (de todas las campañas)
            var followersObtenidos = reportesUnidadActual
                .SelectMany(r => r.Campañas)
                .Sum(c => c.FollowersCount);

            // Total de campañas
            var totalCampañas = reportesUnidadActual.SelectMany(r => r.Campañas).Count();

            resultado.Add(new PublicidadResumenUnidadDto
            {
                UnidadNegocio = unidad,
                GastoTotal = gastoTotal,
                PorcentajeDelTotal = gastoTotalGeneral > 0 ? 
                    Math.Round((gastoTotal / gastoTotalGeneral) * 100, 2) : 0,
                CambioVsMesAnterior = Math.Round(cambioVsAnterior, 2),
                TotalCampañas = totalCampañas,
                Performance = Math.Round(performance, 2),
                FollowersObtenidos = followersObtenidos
            });
        }

        return resultado.OrderByDescending(r => r.GastoTotal).ToList();
    }

    public static List<PublicidadResumenPlataformaDto> CalcularResumenPorPlataforma(
        List<AdReport> reportesActuales)
    {
        var resultado = new List<PublicidadResumenPlataformaDto>();
        var gastoTotalGeneral = reportesActuales.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);

        // Agrupar por plataforma
        var porPlataforma = reportesActuales.GroupBy(r => r.Plataforma);

        foreach (var grupo in porPlataforma)
        {
            var plataforma = grupo.Key;
            var reportes = grupo.ToList();
            var campañas = reportes.SelectMany(r => r.Campañas).ToList();

            var gastoTotal = campañas.Sum(c => c.MontoInvertido);
            var totalCampañas = campañas.Count;

            // Métricas solo para campañas con datos (Meta)
            var campañasConMetricas = campañas.Where(c => c.Clicks > 0 || c.Impressions > 0);
            var totalClicks = campañasConMetricas.Sum(c => c.Clicks);
            var totalImpressions = campañasConMetricas.Sum(c => c.Impressions);
            var ctrPromedio = totalImpressions > 0 ? 
                (decimal)totalClicks / totalImpressions * 100 : 0;

            // Determinar si es automático (Meta tiene métricas automáticas)
            var esAutomatico = plataforma.Equals("Meta", StringComparison.OrdinalIgnoreCase) && 
                                campañasConMetricas.Any();

            resultado.Add(new PublicidadResumenPlataformaDto
            {
                Plataforma = plataforma,
                GastoTotal = gastoTotal,
                PorcentajeDelTotal = gastoTotalGeneral > 0 ? 
                    Math.Round((gastoTotal / gastoTotalGeneral) * 100, 2) : 0,
                TotalCampañas = totalCampañas,
                CtrPromedio = Math.Round(ctrPromedio, 2),
                TotalClicks = totalClicks,
                TotalImpressions = totalImpressions,
                EsAutomatico = esAutomatico
            });
        }

        return resultado.OrderByDescending(r => r.GastoTotal).ToList();
    }

    public static PublicidadComparativaDto CalcularComparativa(
        List<AdReport> reportesActuales,
        List<AdReport> reportesAnteriores, 
        DateTime fechaActual)
    {
        var gastoActual = reportesActuales.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);
        var gastoAnterior = reportesAnteriores.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido);

        var diferencia = gastoActual - gastoAnterior;
        var porcentajeCambio = gastoAnterior > 0 ? (diferencia / gastoAnterior) * 100 : 0;

        var tendencia = porcentajeCambio switch
        {
            > 5 => "aumento",
            < -5 => "disminucion", 
            _ => "estable"
        };

        // Mejor unidad (mayor gasto actual)
        var mejorUnidad = reportesActuales
            .GroupBy(r => r.UnidadNegocio)
            .OrderByDescending(g => g.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido))
            .FirstOrDefault()?.Key ?? "N/A";

        // Mejor plataforma (mayor gasto actual)
        var mejorPlataforma = reportesActuales
            .GroupBy(r => r.Plataforma)
            .OrderByDescending(g => g.SelectMany(r => r.Campañas).Sum(c => c.MontoInvertido))
            .FirstOrDefault()?.Key ?? "N/A";

        var fechaAnterior = fechaActual.AddMonths(-1);

        return new PublicidadComparativaDto
        {
            MesActual = fechaActual.Month,
            AñoActual = fechaActual.Year,
            MesAnterior = fechaAnterior.Month,
            AñoAnterior = fechaAnterior.Year,
            DiferenciaGasto = Math.Round(diferencia, 2),
            PorcentajeCambio = Math.Round(porcentajeCambio, 2),
            Tendencia = tendencia,
            MejorUnidad = mejorUnidad,
            MejorPlataforma = mejorPlataforma
        };
    }

    public static List<TopCampañaDto> ObtenerTopCampañas(
        List<AdReport> reportesActuales, 
        int limit = 10)
    {
        var todasLasCampañas = reportesActuales
            .SelectMany(r => r.Campañas.Select(c => new { Reporte = r, Campaña = c }))
            .ToList();

        // Calcular performance para cada campaña
        var campañasConPerformance = todasLasCampañas
            .Select(item => new
            {
                item.Reporte,
                item.Campaña,
                Performance = CalcularPerformanceCampaña(item.Campaña)
            })
            .OrderByDescending(x => x.Performance)
            .Take(limit)
            .ToList();

        var resultado = campañasConPerformance
            .Select(item => new TopCampañaDto
            {
                CampaignId = item.Campaña.CampaignId,
                Nombre = item.Campaña.Nombre,
                UnidadNegocio = item.Reporte.UnidadNegocio,
                Plataforma = item.Reporte.Plataforma,
                MontoInvertido = item.Campaña.MontoInvertido,
                Performance = Math.Round(item.Performance, 2),
                Clicks = item.Campaña.Clicks,
                Impressions = item.Campaña.Impressions,
                Ctr = item.Campaña.Ctr,
                Reach = item.Campaña.Reach,
                TipoFuente = DeterminarTipoFuente(item.Campaña),
                FechaInicio = item.Campaña.FechaInicio,
                FechaFin = item.Campaña.FechaFin
            })
            .ToList();

        return resultado;
    }

    private static decimal CalcularPerformanceCampaña(AdCampaign campaña)
    {
        // Performance = (CTR * Reach) / Gasto
        // Si no hay métricas automáticas, usar FollowersCount como performance básica
        
        if (campaña.Clicks > 0 && campaña.Impressions > 0 && campaña.Reach > 0 && campaña.MontoInvertido > 0)
        {
            var ctr = (decimal)campaña.Clicks / campaña.Impressions * 100;
            return (ctr * campaña.Reach) / campaña.MontoInvertido;
        }
        
        // Para campañas manuales, usar followers/gasto como performance básica
        if (campaña.FollowersCount > 0 && campaña.MontoInvertido > 0)
        {
            return campaña.FollowersCount / campaña.MontoInvertido * 100;
        }

        return 0;
    }

    private static string DeterminarTipoFuente(AdCampaign campaña)
    {
        // Si tiene métricas de Meta (clicks, impressions), es automático
        if (campaña.Clicks > 0 || campaña.Impressions > 0 || campaña.Reach > 0)
            return "automatico";
        
        return "manual";
    }
}
