using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AdCampaignConfiguration : IEntityTypeConfiguration<AdCampaign>
{
    public void Configure(EntityTypeBuilder<AdCampaign> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre).IsRequired();
        builder.Property(c => c.Tipo).IsRequired();
        builder.Property(c => c.MontoInvertido).IsRequired();
        builder.Property(c => c.Objetivo).IsRequired();
        builder.Property(c => c.Resultados).IsRequired();
        builder.Property(c => c.FechaInicio).IsRequired();
        builder.Property(c => c.FechaFin).IsRequired();
    }
}
