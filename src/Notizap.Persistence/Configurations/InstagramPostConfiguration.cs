using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InstagramPostConfiguration : IEntityTypeConfiguration<InstagramPost>
{
    public void Configure(EntityTypeBuilder<InstagramPost> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.PostId).IsUnique();

        builder.Property(p => p.Cuenta).IsRequired();
        builder.Property(p => p.Url).IsRequired();
        builder.Property(p => p.ImageUrl);
        builder.Property(p => p.Content).HasMaxLength(2000).IsRequired(false);

        builder.Property(p => p.FechaPublicacion);
        builder.Property(p => p.Likes);
        builder.Property(p => p.Comments);
        builder.Property(p => p.Shares);
        builder.Property(p => p.Interactions);
        builder.Property(p => p.Engagement);
        builder.Property(p => p.Impressions);
        builder.Property(p => p.Reach);
        builder.Property(p => p.Saved);
        builder.Property(p => p.VideoViews);
        builder.Property(p => p.Clicks);
        builder.Property(p => p.BusinessId);
        builder.Property(p => p.CreatedAt);
    }
}
