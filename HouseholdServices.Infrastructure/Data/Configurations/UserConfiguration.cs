using HouseholdServices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace HouseholdServices.Infrastructure.Data.Configurations;

// наследование от этого интерфейса позволяет классу быть конфигуратором для класса User. Интерфейс из Metadata.Builders
// здесь мы задаем ограничения целостности
public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        // задаем как ключ
        // аргумент лямбда функции явно типа User так как тот тип задан в билдере
        // => это лямбда оператор (возьми аргумент слева и сделай с ним то, что справа)
        builder.HasKey(user => user.UserId);

        builder.Property(user => user.UserId)
            .HasColumnName("user_id");

        builder.Property(user => user.Login)
            .HasColumnName("login")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(user => user.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(user => user.Phone)
            .HasColumnName("phone")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // для поля логин создаем индекс, уникальность как следствие, но не делаем это основным идентификатором пользователя
        builder.HasIndex(user => user.Login)
            .IsUnique();
    }
}