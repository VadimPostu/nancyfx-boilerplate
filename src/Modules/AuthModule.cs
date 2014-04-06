using System;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;
using NancyBoilerplate.Web.Core;
using NancyBoilerplate.Web.Entities;

namespace NancyBoilerplate.Web.Modules
{
    public class AuthModule : NancyModule
    {
        private const int CookieLifeSpanInDays = 7;
        private readonly UserMapper _userMapper;

        public AuthModule(UserMapper userMapper)
        {
            _userMapper = userMapper;

            Get["/newuser"] = Get_CreateNewUser;
            Get["/signin"] = _ => View["signin", new SignInViewModel()];
            Post["/signin"] = Post_SignIn;
        }

        private object Get_CreateNewUser(object arg)
        {
            var user = _userMapper.CreateUser("Admin", "d.postu@gmail.com", "password", new[] {"user", "administrator"});

            return Response.AsJson(user);
        }

        private object Post_SignIn(object arg)
        {
            var signInRequest = this.Bind<SignInViewModel>();
            var user = _userMapper.ValidateUser(signInRequest.UserName, signInRequest.Password);
            
            if (user == null)
            {
                return View["signin", signInRequest];
            }

            return this.Login(user.UniqueId, DateTime.Now.AddDays(CookieLifeSpanInDays));
        }

        public class SignInViewModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}