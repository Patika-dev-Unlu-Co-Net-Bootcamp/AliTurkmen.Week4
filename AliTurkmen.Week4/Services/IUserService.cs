using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using jwtCoreDemo.Entities;
using jwtCoreDemo.Helpers;

namespace WebApi.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        IEnumerable<User> Insert(User user);
        bool IsUserExist(User user);
    }

    public class UserService : IUserService
    {
        private List<User> _users = new List<User>
        {
            new User { Id = 1, Name = "Ali", LastName = "Türkmen", UserName = "ali07", Password = "0001" },
            new User { Id = 2, Name = "Deniz", LastName = "Poyraz", UserName = "deniz123", Password = "1000" }
        };

        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public User Authenticate(string kullaniciAdi, string sifre)
        {
            var user = _users.SingleOrDefault(x => x.UserName == kullaniciAdi && x.Password == sifre);

            if (user == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            user.Password = null;

            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _users.Select(x =>
            {
                x.Password = null;
                return x;
            });
        }

        public bool IsUserExist(User user)
        {
            bool isExist;

            var userName = user.UserName.ToLower();
            isExist = _users.Any(n => n.UserName == user.UserName.ToLower());

            return isExist;
        }

        public IEnumerable<User> Insert(User user)
        {
            _users.Add(user);
            return _users;
        }

        
    }
}