using Application.Interfaces.Repository;
using Domain.Data;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public Task<User?> FindByEmailAsync(string email) => _userManager.FindByEmailAsync(email);
    public Task<User?> FindByIdAsync(string id) => _userManager.FindByIdAsync(id);
    public Task<IdentityResult> CreateAsync(User user, string password) => _userManager.CreateAsync(user, password);
    public Task<IdentityResult> AddToRoleAsync(User user, string role) => _userManager.AddToRoleAsync(user, role);
    public Task<bool> CheckPasswordAsync(User user, string password) => _userManager.CheckPasswordAsync(user, password);
    public Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword) =>
        _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
}
