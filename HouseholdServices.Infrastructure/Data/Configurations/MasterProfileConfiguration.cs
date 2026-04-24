using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseholdServices.Infrastructure.Data.Configurations;

public class MasterProfileConfiguration : IEntityTypeConfiguration<MasterProfile>
{
    public void Configure(EntityTypeBuilder<MasterProfile> builder)
    {
        builder.ToTable("master_profiles");

        builder.HasKey(masterProfile => masterProfile.UserId);

        builder.Property(masterProfile => masterProfile.UserId)
            .HasColumnName("user_id");

        builder.Property(masterProfile => masterProfile.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(masterProfile => masterProfile.ExperienceYears)
            .HasColumnName("experience_years")
            .IsRequired();

        builder.HasOne(masterProfile => masterProfile.User)
            .WithOne(user => user.MasterProfile)
            .HasForeignKey<MasterProfile>(masterProfile => masterProfile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
