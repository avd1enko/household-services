namespace HouseholdServices.Application.Exceptions.Request;

public class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException()
        : base("Category not found")
    {
    }
}
