using Microsoft.EntityFrameworkCore;
using HouseholdServices.Domain.Entities;

namespace HouseholdServices.Infrastructure.Data;

// этот файл связывает вместе сущности, конфигурации, подключение к бд, операции чтения и записи. Это наша бд и работа с ней в формате кода
public class HouseholdServicesDbContext : DbContext
{
    public HouseholdServicesDbContext(DbContextOptions<HouseholdServicesDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<Response> Responses { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Review> Reviews { get; set; }

    // метод, где EF Core собирает модель бд. Здесь фреймворк понимает какие есть сущности и какие их атрибуты/ограничения и тд
    // протектед так как переопределяем протектед метод
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //внутри Infrastructure ищет классы, которые реулизуют IEntityTypeConfiguration<T>, иначе бы всё в ручную добавляли
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HouseholdServicesDbContext).Assembly);

        // вызываем реализацию базового DbContext поверх накрученного нами функционала в overrided методе
        base.OnModelCreating(modelBuilder);
    }
}
//DbContext — центральный объект EF Core, через который приложение ходит в db