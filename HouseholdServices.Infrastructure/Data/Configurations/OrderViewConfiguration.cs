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
    }
}