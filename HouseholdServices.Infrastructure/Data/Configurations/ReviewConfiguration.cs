using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdServices.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(review => review.ReviewId);

        builder.Property(review => review.ReviewId)
            .HasColumnName("review_id");

        builder.Property(review => review.OrderId)
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(review => review.Rating)
            .HasColumnName("rating")
            .IsRequired();

        builder.Property(review => review.Comment)
            .HasColumnName("comment")
            .HasMaxLength(1000);

        builder.Property(review => review.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.ToTable(table => table.HasCheckConstraint("CK_reviews_rating_range", "rating >= 1 AND rating <= 5"));

        builder.HasOne(review => review.Order)
            .WithOne(order => order.Review)
            .HasForeignKey<Review>(review => review.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(review => review.OrderId)
            .IsUnique();
    }
}
