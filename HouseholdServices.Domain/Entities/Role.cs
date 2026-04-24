namespace HouseholdServices.Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}


// navigation property for roleId
/* это дает нам возможность обращаться к атрибутам экземпляра таблицы Role,
которая связана с конкретным экземпляром таблицы roleId. Мы можем получать доступ к данным другой таблицы,
используя эту навигационную связь по сути это реализация механизма FK
*/