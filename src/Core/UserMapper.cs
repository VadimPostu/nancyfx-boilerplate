using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using NancyBoilerplate.Web.Entities;
using Raven.Client;

namespace NancyBoilerplate.Web.Core
{
    public class UserMapper : IUserMapper
    {
        private readonly IDocumentSession _session;

        public UserMapper(IDocumentSession session)
        {
            _session = session;
        }

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            var user = _session.Query<User>()
                               .Where(u => u.UniqueId == identifier)
                               .FirstOrDefault();

            return user;
        }

        public User ValidateUser(string userName, string password)
        {
            var encodedPassword = CreateHash(password);
            var user = _session.Query<User>()
                               .Where(u => u.UserName == userName && u.Password == encodedPassword)
                               .FirstOrDefault();

            return user;
        }

        public string CreateHash(string password)
        {
            var crypt = new SHA256Managed();
            var hash = String.Empty;
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));

            return crypto.Aggregate(hash, (current, bit) => current + bit.ToString("x2"));
        }

        public User CreateUser(string username, string email, string password, float amount, string[] claims)
        {
            var user = new User()
                {
                    UniqueId = Guid.NewGuid(),
                    UserName = username,
                    Email = email,
                    Password = CreateHash(password),
                    Amount = amount,
                    Claims = claims
                };

            _session.Store(user);
            _session.SaveChanges();

            return user;
        }
    }
}