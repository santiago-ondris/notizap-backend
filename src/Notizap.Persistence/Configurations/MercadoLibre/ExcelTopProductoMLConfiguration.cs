using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ExcelTopProductoMLConfiguration : IEntityTypeConfiguration<ExcelTopProductoML>
{
    public void Configure(EntityTypeBuilder<ExcelTopProductoML> builder)
    {
        builder.ToTable("ExcelTopProductosML");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ModeloColor).HasMaxLength(200).IsRequired();
    }
}
