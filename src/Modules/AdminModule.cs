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

            this.RequiresClaims(new[] { "administrator" });

            Get["/"] = Get_Admin;
            Get["/users"] = Get_Users;
            Get["/users/add"] = Get_UsersAdd;
            Post["/users/add"] = Post_UsersAdd;

            Get["/users/{uniqueId}"] = args => Get_UserEdit(args.uniqueId);
            Post["/users/{uniqueId}"] = args => Post_UserEdit(args.uniqueId);

            Get["/users/{uniqueId}/delete"] = args => Get_UserDelete(args.uniqueId);
        }

        private dynamic Post_UsersAdd(dynamic arg)
        {
            var viewModel = this.Bind<AddOrEditUserViewModel>();
            var user = _userMapper.CreateUser(viewModel.UserName, viewModel.Email, viewModel.Password, viewModel.Amount, new[] { "user" });

            return Response.AsRedirect("~/admin/users");
        }

        private dynamic Get_UsersAdd(dynamic arg)
        {
            return View[new AddOrEditUserViewModel()];
        }

        private dynamic Get_Users(dynamic arg)
        {
            var users = _session.Query<User>()
                .Customize(q => q.WaitForNonStaleResults())
                .ToArray();

            return View[new UsersListViewModel(users)];
        }

        private dynamic Post_UserEdit(Guid uniqueId)
        {
            var viewModel = this.Bind<AddOrEditUserViewModel>();
            User user = _session.Query<User>().Where(u => u.UniqueId == uniqueId).FirstOrDefault();

            user.Amount = viewModel.Amount;
            user.Email = viewModel.Email;
            user.UserName = viewModel.UserName;
            if (!String.IsNullOrEmpty(viewModel.Password))
            {
                user.Password =  _userMapper.CreateHash(viewModel.Password);
            }

            _session.SaveChanges();

            return Response.AsRedirect("~/admin/users");
        }

        private dynamic Get_UserEdit(Guid uniqueId)
        {
            User user = _session.Query<User>().Where(u => u.UniqueId == uniqueId).FirstOrDefault();
            
            if (user != null)
            {
                AddOrEditUserViewModel model = new AddOrEditUserViewModel();

                model.UserName = user.UserName;
                model.Email = user.Email;
                model.Amount = user.Amount;
                model.User = user;

                return View[model];
            }
            else
            {
                return Response.AsRedirect("~/admin/users");
            }
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

        public class AddOrEditUserViewModel
        {
            public User User { get; set; }

            public string UserName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public float Amount { get; set; }
        }
    }
}