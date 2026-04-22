using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdServices.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(order => order.OrderId);

        builder.Property(order => order.OrderId)
            .HasColumnName("order_id");

        builder.Property(order => order.ResponseId)
            .HasColumnName("response_id")
            .IsRequired();

        builder.Property(order => order.OrderStatusId)
            .HasColumnName("order_status_id")
            .IsRequired();

        builder.Property(order => order.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(order => order.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(order => order.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasOne(order => order.Response)
            .WithOne(response => response.Order)
            .HasForeignKey<Order>(order => order.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(order => order.ResponseId)
            .IsUnique();

        builder.HasIndex(order => order.OrderStatusId);
    }
}
