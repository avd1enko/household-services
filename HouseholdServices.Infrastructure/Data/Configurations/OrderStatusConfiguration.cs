using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HouseholdServices.Infrastructure.Data.Configurations;

public class OrderStatusConfiguration: IEntityTypeConfiguration<OrderStatus>
{
    public void Configure(EntityTypeBuilder<OrderStatus> builder)
    {
        builder.ToTable("order_statuses");
        
        builder.HasKey(order => order.OrderStatusId);

        builder.Property(order => order.OrderStatusId)
            .HasColumnName("order_status_id");
        
        builder.Property(order => order.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(order => order.Name)
            .IsUnique();
    }
}