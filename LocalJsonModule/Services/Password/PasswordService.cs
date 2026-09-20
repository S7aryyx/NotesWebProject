using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace LocalJsonModule.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> _passwordHasher;

    public PasswordService()
    {
        _passwordHasher = new PasswordHasher<object>();
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be empty.");
        }
        return _passwordHasher.HashPassword(null!,password);
    }

    public bool VerifyPassword(string password,string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }
        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(null!,passwordHash,password);
        return (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded);
    }
}