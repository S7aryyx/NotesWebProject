using LocalJsonModule.DTOs.Users;
using LocalJsonModule.Models;
using LocalJsonModule.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalJsonModule.Services.Update
{
    public class UpdateService : IUpdateService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UpdateService(
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }


        public async Task<bool> UpdateAsync(Guid id, UpdateRequest request)
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

            User? user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return false;
            }

            User? existingLogin = await _userRepository.GetByLoginAsync(request.Login);

            if (existingLogin != null && existingLogin.Id != id)
            {
                throw new InvalidOperationException("Пользователь с таким логином уже существует.");
            }

            User? existingEmail = await _userRepository.GetByEmailAsync(request.Email);

            if (existingEmail != null && existingEmail.Id != id)
            {
                throw new InvalidOperationException("Пользователь с таким email уже существует.");
            }

            user.Login = request.Login;
            user.Email = request.Email;
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
