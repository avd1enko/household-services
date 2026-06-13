namespace HouseholdServices.Application.Exceptions.Order;

public class InvalidOrderInitialMeetingDateException : Exception
{
    public InvalidOrderInitialMeetingDateException()
        : base("Initial meeting date must be in the future.")
    {
    }
}