using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InstagramReelConfiguration : IEntityTypeConfiguration<InstagramReel>
{
    public void Configure(EntityTypeBuilder<InstagramReel> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.ReelId).IsUnique();

        builder.Property(r => r.Cuenta).IsRequired();
        builder.Property(r => r.Url).IsRequired();
        builder.Property(r => r.ImageUrl).IsRequired();
        builder.Property(r => r.Contenido).HasMaxLength(2000);

        builder.Property(r => r.FechaPublicacion);
        builder.Property(r => r.Likes);
        builder.Property(r => r.Comentarios);
        builder.Property(r => r.Views);
        builder.Property(r => r.Engagement);
        builder.Property(r => r.Interacciones);
        builder.Property(r => r.VideoViews);
        builder.Property(r => r.Guardados);
        builder.Property(r => r.Compartidos);
        builder.Property(r => r.Reach);
        builder.Property(r => r.BusinessId);
        builder.Property(r => r.CreatedAt);
    }
}
