using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AnuncioDisplayMLConfiguration : IEntityTypeConfiguration<AnuncioDisplayML>
{
    public void Configure(EntityTypeBuilder<AnuncioDisplayML> builder)
    {
        builder.ToTable("AnunciosDisplayML");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(a => a.Ctr)
            .HasColumnType("decimal(5,2)");
    }
}
