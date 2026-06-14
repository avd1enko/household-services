using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdServices.Infrastructure.Data.Configurations;

public class OrderViewConfiguration : IEntityTypeConfiguration<OrderView>
{
    public void Configure(EntityTypeBuilder<OrderView> builder)
    {
        builder.HasNoKey();
        builder.ToView("order_details_view");

        builder.Property(order => order.OrderId).HasColumnName("order_id");
        builder.Property(order => order.Status).HasColumnName("status");
        builder.Property(order => order.Price).HasColumnName("price");
        builder.Property(order => order.InitialMeetingAt).HasColumnName("initial_meeting_at");
        builder.Property(order => order.CreatedAt).HasColumnName("created_at");
        builder.Property(order => order.CompletedAt).HasColumnName("completed_at");

        builder.Property(order => order.RequestId).HasColumnName("request_id");
        builder.Property(order => order.RequestTitle).HasColumnName("request_title");
        builder.Property(order => order.RequestDescription).HasColumnName("request_description");
        builder.Property(order => order.RequestAddress).HasColumnName("request_address");
        builder.Property(order => order.DesiredDate).HasColumnName("desired_date");
        builder.Property(order => order.CategoryId).HasColumnName("category_id");
        builder.Property(order => order.CategoryName).HasColumnName("category_name");

        builder.Property(order => order.ClientId).HasColumnName("client_id");
        builder.Property(order => order.ClientFirstName).HasColumnName("client_first_name");
        builder.Property(order => order.ClientLastName).HasColumnName("client_last_name");
        builder.Property(order => order.ClientPhone).HasColumnName("client_phone");

        builder.Property(order => order.MasterId).HasColumnName("master_id");
        builder.Property(order => order.MasterFirstName).HasColumnName("master_first_name");
        builder.Property(order => order.MasterLastName).HasColumnName("master_last_name");
        builder.Property(order => order.MasterPhone).HasColumnName("master_phone");
    }
}