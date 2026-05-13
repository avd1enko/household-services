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

        builder.HasData(
            new ServiceCategory
            {
                CategoryId = 1,
                Name = "plumbing",
                Description = "Plumbing installation, repairs, and maintenance."
            },
            new ServiceCategory
            {
                CategoryId = 2,
                Name = "electrical",
                Description = "Electrical installation, diagnostics, and repairs."
            },
            new ServiceCategory
            {
                CategoryId = 3,
                Name = "cleaning",
                Description = "Regular, deep, and post-renovation cleaning."
            },
            new ServiceCategory
            {
                CategoryId = 4,
                Name = "appliance_repair",
                Description = "Diagnostics and repair of household appliances."
            },
            new ServiceCategory
            {
                CategoryId = 5,
                Name = "furniture_assembly",
                Description = "Furniture assembly, installation, and minor repairs."
            });
    }
}
