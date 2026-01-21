namespace Application.Interfaces;

public interface IAuthenticableUser : IUser
{
    string PasswordHash { get; set; }
}
