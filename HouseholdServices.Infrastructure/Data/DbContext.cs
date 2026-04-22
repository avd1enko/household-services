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
    
    public DbSet<User> Users { get; set; } // по смыслу соответствует таблицам users (набор сущностей users, который маппится на таблицу users)
    public DbSet<Role> Roles { get; set; } 
    public DbSet<ResponseStatus> ResponseStatuses { get; set; }
    public DbSet<RequestStatus> RequestStatuses { get; set; }
    public DbSet<OrderStatus> OrderStatuses { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    

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