using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdServices.Infrastructure.Data.Configurations;

public class ResponseConfiguration : IEntityTypeConfiguration<Response>
{
    public void Configure(EntityTypeBuilder<Response> builder)
    {
        builder.ToTable("responses");

        builder.HasKey(response => response.ResponseId);

        builder.Property(response => response.ResponseId)
            .HasColumnName("response_id");

        builder.Property(response => response.RequestId)
            .HasColumnName("request_id")
            .IsRequired();

        builder.Property(response => response.MasterId)
            .HasColumnName("master_id")
            .IsRequired();

        builder.Property(response => response.ResponseStatusId)
            .HasColumnName("response_status_id")
            .IsRequired();

        builder.Property(response => response.ProposedPrice)
            .HasColumnName("proposed_price")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(response => response.Comment)
            .HasColumnName("comment")
            .HasMaxLength(1000);

        builder.Property(response => response.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(response => response.Request)
            .WithMany(request => request.Responses)
            .HasForeignKey(response => response.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(response => response.Master)
            .WithMany(user => user.MasterResponses)
            .HasForeignKey(response => response.MasterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(response => new { response.RequestId, response.MasterId })
            .IsUnique();

        builder.HasIndex(response => response.ResponseStatusId);
    }
}
