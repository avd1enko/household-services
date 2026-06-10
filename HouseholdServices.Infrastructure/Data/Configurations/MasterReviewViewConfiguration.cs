using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdServices.Infrastructure.Data.Configurations;

public class MasterReviewViewConfiguration : IEntityTypeConfiguration<MasterReviewView>
{
    public void Configure(EntityTypeBuilder<MasterReviewView> builder)
    {
        builder.HasNoKey();
        builder.ToView("master_reviews_view");

        builder.Property(review => review.ReviewId).HasColumnName("review_id");
        builder.Property(review => review.OrderId).HasColumnName("order_id");
        builder.Property(review => review.Rating).HasColumnName("rating");
        builder.Property(review => review.Comment).HasColumnName("comment");
        builder.Property(review => review.ReviewCreatedAt).HasColumnName("review_created_at");
        builder.Property(review => review.OrderCompletedAt).HasColumnName("order_completed_at");
        builder.Property(review => review.RequestId).HasColumnName("request_id");
        builder.Property(review => review.RequestTitle).HasColumnName("request_title");
        builder.Property(review => review.CategoryId).HasColumnName("category_id");
        builder.Property(review => review.CategoryName).HasColumnName("category_name");
        builder.Property(review => review.ClientId).HasColumnName("client_id");
        builder.Property(review => review.ClientFirstName).HasColumnName("client_first_name");
        builder.Property(review => review.ClientLastName).HasColumnName("client_last_name");
        builder.Property(review => review.MasterId).HasColumnName("master_id");
        builder.Property(review => review.MasterFirstName).HasColumnName("master_first_name");
        builder.Property(review => review.MasterLastName).HasColumnName("master_last_name");
    }
}
