using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InstagramSeguidoresConfiguration : IEntityTypeConfiguration<InstagramSeguidores>
{
    public void Configure(EntityTypeBuilder<InstagramSeguidores> builder)
    {
        builder.HasKey(x => x.Id);

        // Si querés evitar datos duplicados para la misma cuenta en la misma fecha
        builder.HasIndex(x => new { x.Cuenta, x.Date }).IsUnique();

        builder.Property(x => x.Cuenta)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Value)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
