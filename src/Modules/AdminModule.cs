using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using NancyBoilerplate.Web.Entities;
using Raven.Client;
using System;
using System.Linq;
using NancyBoilerplate.Web.Core;

namespace NancyBoilerplate.Web.Modules
{
    public class AdminModule : NancyModule
    {
        private readonly UserMapper _userMapper;

        private readonly IDocumentSession _session;

        public AdminModule(IDocumentSession session, UserMapper userMapper)
            : base("/admin")
        {
            _session = session;
            _userMapper = userMapper;

            //this.RequiresClaims(new[] { "administrator" });

            Get["/"] = Get_Admin;
            Get["/users"] = Get_Users;
            Get["/users/add"] = Get_UsersAdd;
            Post["/users/add"] = Post_UsersAdd;
            Get["/users/{uniqueId}"] = args => Get_User(args.uniqueId);
            Get["/users/{uniqueId}/delete"] = args => Get_UserDelete(args.uniqueId);
        }

        private dynamic Post_UsersAdd(dynamic arg)
        {
            var viewModel = this.Bind<AddUserViewModel>();
            var user = _userMapper.CreateUser(viewModel.UserName, viewModel.Email, viewModel.Password, viewModel.Amount, new[] { "user" });

            return Response.AsRedirect("~/admin/users");
        }

        private dynamic Get_UsersAdd(dynamic arg)
        {
            return View[new AddUserViewModel()];
        }

        private dynamic Get_Users(dynamic arg)
        {
            var users = _session.Query<User>().ToArray();

            return View[new UsersListViewModel(users)];
        }

        private dynamic Get_User(Guid uniqueId)
        {
            return uniqueId.ToString();
        }

        private dynamic Get_UserDelete(Guid uniqueId)
        {
            User user = _session.Query<User>().Where(u => u.UniqueId == uniqueId).FirstOrDefault();
            
            if (user != null && !user.HasClaim("administrator")) 
            {
                _session.Delete(user);
                _session.SaveChanges();
            }

            return Response.AsRedirect("~/admin/users");
        }

        private object Get_Admin(object arg)
        {
            return "administrators only";
        }

        public class UsersListViewModel
        {
            public User[] Users { get; set; }

            public UsersListViewModel(User[] users)
            {
                Users = users ?? new User[] {};
            }
        }

        public class AddUserViewModel
        {
            public string UserName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public float Amount { get; set; }
        }
    }

    public static class Extensions
    {
        public static string GetX(this User user)
        {
            return user.ToString();
        }
    }
}