using LocalJsonModule.DTOs.Users;
using LocalJsonModule.Models;
using LocalJsonModule.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalJsonModule.Services.Register
{
    public class RegisterService : IRegisterService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public RegisterService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> CreateAsync(RegisterRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Данные пользователя не могут быть null.");
            }

            if (request.Login == null || request.Email == null || request.Password == null)
            {
                throw new ArgumentException("Логин, email и пароль не могут быть null.");
            }

            if (request.Login == "" || request.Email == "" || request.Password == "")
            {
                throw new ArgumentException("Логин, email и пароль не могут быть пустыми.");
            }

            User? existingLogin = await _userRepository.GetByLoginAsync(request.Login);

            if (existingLogin != null)
            {
                throw new InvalidOperationException("Пользователь с таким логином уже существует.");
            }

            User? existingEmail = await _userRepository.GetByEmailAsync(request.Email);

            if (existingEmail != null)
            {
                throw new InvalidOperationException("Пользователь с таким email уже существует.");
            }

            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Login = request.Login,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddAsync(user);
            return user;
        }
    }
}
