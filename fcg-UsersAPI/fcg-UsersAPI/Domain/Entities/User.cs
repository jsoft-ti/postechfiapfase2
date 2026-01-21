using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public User() : base() {
        UserName ??= Email;
    }
}