using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InstagramStoryConfiguration : IEntityTypeConfiguration<InstagramStory>
{
    public void Configure(EntityTypeBuilder<InstagramStory> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => s.PostId).IsUnique();

        builder.Property(s => s.Cuenta).IsRequired();
        builder.Property(s => s.MediaUrl).IsRequired(false);
        builder.Property(s => s.ThumbnailUrl);
        builder.Property(s => s.Permalink);
        builder.Property(s => s.Content).HasMaxLength(1000).IsRequired(false);

        builder.Property(s => s.FechaPublicacion);
        builder.Property(s => s.Impressions);
        builder.Property(s => s.Reach);
        builder.Property(s => s.Replies);
        builder.Property(s => s.TapsForward);
        builder.Property(s => s.TapsBack);
        builder.Property(s => s.Exits);
        builder.Property(s => s.BusinessId);
        builder.Property(s => s.CreatedAt);
    }
}
