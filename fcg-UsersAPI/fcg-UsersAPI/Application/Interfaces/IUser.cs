namespace Application.Interfaces;

public interface IUser
{
    string FirstName { get; set; } 
    string LastName { get; set; } 
    string FullName { get; }
    string DisplayName { get; set; }
    DateTime Birthday { get; set; }
}
