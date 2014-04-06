using System;
using Nancy;
using Nancy.Security;
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

            Get["/signin"] = _ => View["signin", new SignInViewModel()];
            Post["/signin"] = Post_SignIn;
            Get["/signout"] = Get_SignOut;
        }

        private dynamic Get_SignOut(dynamic arg)
        {
            return this.Logout("~/signin");
        }

        private object Post_SignIn(object arg)
        {
            var signInRequest = this.Bind<SignInViewModel>();
            var user = _userMapper.ValidateUser(signInRequest.UserName, signInRequest.Password);
            String redirectUrl = "";
            
            if (user == null)
            {
                return View["signin", signInRequest];
            }
            if(user.HasClaim("administrator")) redirectUrl = "~/admin/users";
            else redirectUrl = "~/user";

            return this.Login(user.UniqueId, DateTime.Now.AddDays(CookieLifeSpanInDays), redirectUrl);
        }

        public class SignInViewModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}