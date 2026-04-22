using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HouseholdServices.Infrastructure.Data.Configurations;

public class ServiceCategoryConfiguration: IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("service_categories");
        
        builder.HasKey(category => category.CategoryId);

        builder.Property(category => category.CategoryId)
            .HasColumnName("category_id");
        
        builder.Property(category => category.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(category => category.Description)
            .HasColumnName("description")
            .HasMaxLength(500);
        
        builder.HasIndex(category => category.Name)
            .IsUnique();
    }
}