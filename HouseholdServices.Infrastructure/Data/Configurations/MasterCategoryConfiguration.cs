using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdServices.Infrastructure.Data.Configurations;

public class MasterCategoryConfiguration : IEntityTypeConfiguration<MasterCategory>
{
    public void Configure(EntityTypeBuilder<MasterCategory> builder)
    {
        builder.ToTable("master_categories");

        builder.HasKey(masterCategory => new { masterCategory.UserId, masterCategory.CategoryId });

        builder.Property(masterCategory => masterCategory.UserId)
            .HasColumnName("user_id");

        builder.Property(masterCategory => masterCategory.CategoryId)
            .HasColumnName("category_id");

        builder.HasOne(masterCategory => masterCategory.User)
            .WithMany(user => user.MasterCategories)
            .HasForeignKey(masterCategory => masterCategory.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(masterCategory => masterCategory.Category)
            .WithMany(category => category.MasterCategories)
            .HasForeignKey(masterCategory => masterCategory.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
