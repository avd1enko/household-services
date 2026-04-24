using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdServices.Infrastructure.Data.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("requests");

        builder.HasKey(request => request.RequestId);

        builder.Property(request => request.RequestId)
            .HasColumnName("request_id");

        builder.Property(request => request.ClientId)
            .HasColumnName("client_id")
            .IsRequired();

        builder.Property(request => request.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(request => request.RequestStatusId)
            .HasColumnName("request_status_id")
            .IsRequired();

        builder.Property(request => request.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(request => request.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(request => request.Address)
            .HasColumnName("address")
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(request => request.DesiredDate)
            .HasColumnName("desired_date")
            .IsRequired();

        builder.Property(request => request.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(request => request.Client)
            .WithMany(user => user.ClientRequests)
            .HasForeignKey(request => request.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => request.ClientId);
        builder.HasIndex(request => request.CategoryId);
        builder.HasIndex(request => request.RequestStatusId);
    }
}
