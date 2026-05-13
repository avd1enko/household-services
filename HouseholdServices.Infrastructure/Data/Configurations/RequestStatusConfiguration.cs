using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HouseholdServices.Infrastructure.Data.Configurations;

public class RequestStatusConfiguration: IEntityTypeConfiguration<RequestStatus>
{
    public void Configure(EntityTypeBuilder<RequestStatus> builder)
    {
        builder.ToTable("request_statuses");
        
        builder.HasKey(request => request.RequestStatusId);

        builder.Property(request => request.RequestStatusId)
            .HasColumnName("request_status_id");
        
        builder.Property(request => request.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(request => request.Name)
            .IsUnique();

        builder.HasData(
            new RequestStatus { RequestStatusId = 1, Name = "open" },
            new RequestStatus { RequestStatusId = 2, Name = "in_progress" },
            new RequestStatus { RequestStatusId = 3, Name = "completed" },
            new RequestStatus { RequestStatusId = 4, Name = "cancelled" });
    }
}
