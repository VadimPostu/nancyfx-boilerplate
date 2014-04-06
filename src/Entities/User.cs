using System;
using System.Collections.Generic;
using Nancy.Security;

namespace NancyBoilerplate.Web.Entities
{
    public class User : IUserIdentity
    {
        public Guid UniqueId { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public IEnumerable<string> Claims { get; set; }
    }
}