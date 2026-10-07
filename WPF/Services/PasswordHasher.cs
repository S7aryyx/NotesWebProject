
using Microsoft.AspNetCore.Identity;
using WPF.Models;

namespace WPF.Services;

public class PasswordHasher
{
    private PasswordHasher<User> _passwordHasher;

    public PasswordHasher()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.");
        }
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(null!, passwordHash, password);
        return (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded);
    }
}