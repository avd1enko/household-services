using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HouseholdServices.Infrastructure.Data.Configurations;

public class ResponseStatusConfiguration: IEntityTypeConfiguration<ResponseStatus>
{
    public void Configure(EntityTypeBuilder<ResponseStatus> builder)
    {
        builder.ToTable("response_statuses");
        
        builder.HasKey(response => response.ResponseStatusId);

        builder.Property(response => response.ResponseStatusId)
            .HasColumnName("response_status_id");
        
        builder.Property(response => response.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(response => response.Name)
            .IsUnique();
    }
}