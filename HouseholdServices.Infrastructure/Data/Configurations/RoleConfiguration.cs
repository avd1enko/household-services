using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HouseholdServices.Infrastructure.Data.Configurations;


public class RoleConfiguration: IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        
        builder.HasKey(role => role.RoleId);
        
        builder.Property(role => role.RoleId)
            .HasColumnName("RoleId");
        
        builder.Property(role => role.Name)
            .HasColumnName("Name")
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(role => role.Name)
            .IsUnique();
    }
}