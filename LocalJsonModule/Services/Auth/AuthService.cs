using LocalJsonModule.Models;
using LocalJsonModule.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalJsonModule.DTOs.Users;

namespace LocalJsonModule.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> LoginAsync(LoginRequest request)
        {
            if (request == null)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(request.Login))
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return null;
            }

            User? user = await _userRepository.GetByLoginAsync(request.Login);

            if (user == null)
                return null;

            PasswordVerificationResult result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password);

            if (result == PasswordVerificationResult.Failed)
                return null;

            return user;
        }
    }
}
