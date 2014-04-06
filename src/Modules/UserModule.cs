using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using NancyBoilerplate.Web.Entities;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NancyBoilerplate.Web.Core;

namespace NancyBoilerplate.Web.Modules
{
    public class UserModule : NancyModule
    {
        const String PasswordWrong = "Your password is invalid";
        const String PasswordsDontMatch = "The passwords you introduced don't match";
        const String PasswordNull = "New Password Null";

        private readonly IDocumentSession _session;

        private readonly UserMapper _userMapper;

        public UserModule(IDocumentSession session, UserMapper userMapper)
            : base("/user")
        {
            _session = session;
            _userMapper = userMapper;
            
            this.RequiresClaims(new[] { "user" });

            Get["/"] = Get_Transactions;

            Get["/changePassword"] = Get_ChangePassword;
            Post["/changePassword"] = Post_ChangePassword;

            Get["/transaction"] = Get_ExecuteTransaction;
            Post["/transaction"] = Post_ExecuteTransaction;


        }

        private dynamic Post_ExecuteTransaction(dynamic arg)
        {
            throw new NotImplementedException();
        }

        private dynamic Get_ExecuteTransaction(dynamic arg)
        {
            throw new NotImplementedException();
        }

        private dynamic Post_ChangePassword(dynamic arg)
        {
            Guid uniqueId = (Context.CurrentUser as User).UniqueId;
            var viewModel = this.Bind<ChangePasswordViewModel>();

            User user = _session.Query<User>().Where(u => u.UniqueId == uniqueId).FirstOrDefault();

            if (viewModel.NewPassword != null)
            {
                if (user.Password != _userMapper.CreateHash(viewModel.CurrentPassword))
                {
                    viewModel.CurrentPasswordError = PasswordWrong;
                    return View[viewModel];
                }
                if (viewModel.NewPassword != viewModel.RepeatPassword)
                {
                    viewModel.CurrentPasswordError = PasswordsDontMatch;
                    return View[viewModel];
                }


                user.Password = _userMapper.CreateHash(viewModel.NewPassword);
                _session.SaveChanges();
                return Response.AsRedirect("~/user");
            }
            
            viewModel.NewPasswordError = PasswordNull;
            return View[viewModel];
        }

        private dynamic Get_ChangePassword(dynamic arg)
        {
            return View[new ChangePasswordViewModel()];
        }

        private dynamic Get_Transactions(dynamic arg)
        {
            Guid uniqueId = (Context.CurrentUser as User).UniqueId;

            var transaction = _session.Query<Transaction>().
                Customize(q => q.WaitForNonStaleResults()).
                Where(t => t.Receiver.UniqueId == uniqueId || t.Sender.UniqueId == uniqueId).
                ToArray();

            return View[new TransactionListViewModel(transaction)];
        }

        public class TransactionListViewModel
        {
            public Transaction[] Transaction { get; set; }

            public TransactionListViewModel(Transaction[] transaction)
            {
                Transaction = transaction ?? new Transaction[] { };
            }
        }

        public class ChangePasswordViewModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string RepeatPassword { get; set; }

            public string CurrentPasswordError { get; set; }
            public string RepeatPasswordError { get; set; }
            public string NewPasswordError { get; set; }
        }

    }
}