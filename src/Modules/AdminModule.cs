using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using NancyBoilerplate.Web.Entities;
using Raven.Client;
using System;
using System.Linq;
using NancyBoilerplate.Web.Core;
using System.Collections.Generic;

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

            Get["/users/view/{uniqueId}"] = args => Get_UsersTransactions(args.uniqueId);
            Get["/users/transactions"] = Get_Transactions;

            Get["/users/{uniqueId}"] = args => Get_UserEdit(args.uniqueId);
            Post["/users/{uniqueId}"] = args => Post_UserEdit(args.uniqueId);

            Get["/users/{uniqueId}/delete"] = args => Get_UserDelete(args.uniqueId);
        }

        private dynamic Get_Transactions(dynamic arg)
        {
            var transaction = _session.Query<Transaction>().
                Customize(q => q.WaitForNonStaleResults()).
                ToArray();

            return View[new NancyBoilerplate.Web.Modules.UserModule.TransactionListViewModel(transaction)];
        }

        private dynamic Get_UsersTransactions(Guid uniqueId)
        {
            var transaction = _session.Query<Transaction>().
                Customize(q => q.WaitForNonStaleResults()).
                Where(t => t.Receiver.UniqueId == uniqueId || t.Sender.UniqueId == uniqueId).
                ToArray();

            return View[new NancyBoilerplate.Web.Modules.UserModule.TransactionListViewModel(transaction)];
        }

        private dynamic Post_UsersAdd(dynamic arg)
        {
            var viewModel = this.Bind<AddOrEditUserViewModel>();

            if (string.IsNullOrEmpty(viewModel.UserName) || string.IsNullOrEmpty(viewModel.Password))
            {
                viewModel.IsErrorInForm = true;
                return View[viewModel];
            }
            
            var existingUserWithSameEmail = _session.Query<User>().Where(u => u.Email == viewModel.Email).FirstOrDefault();
            var existingUserWithSameUserName = _session.Query<User>().Where(u => u.UserName == viewModel.UserName).FirstOrDefault();

            if (existingUserWithSameEmail != null || existingUserWithSameUserName != null)
            {
                viewModel.IsErrorInForm = true;
                return View[viewModel];
            }

            List<string> claims = new List<string> { "user" };
            if (viewModel.Claim == "adminClaim")
            {
                claims.Add("administrator");
            }

            var user = _userMapper.CreateUser(viewModel.UserName, viewModel.Email, viewModel.Password, viewModel.Amount, claims.ToArray());
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


            if (viewModel.Amount < 0 ||
             _session.Query<User>().Where(u => (u.Email == viewModel.Email && u.UniqueId != user.UniqueId)).FirstOrDefault() != null ||
             _session.Query<User>().Where(u => (u.UserName == viewModel.UserName && u.UniqueId != user.UniqueId)).FirstOrDefault() != null)
            {
                viewModel.IsErrorInForm = true;
                return View[viewModel];
            }

            user.Amount = viewModel.Amount;
            user.Email = viewModel.Email;
            user.UserName = viewModel.UserName;
            if (!String.IsNullOrEmpty(viewModel.Password))
            {
                user.Password = _userMapper.CreateHash(viewModel.Password);
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

            public String Claim { get; set; }

            public bool IsErrorInForm { get; set; }
        }

    }
}