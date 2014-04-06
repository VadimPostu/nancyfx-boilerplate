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
using Raven.Abstractions.Exceptions;
using System.Threading;

namespace NancyBoilerplate.Web.Modules
{
    public class UserModule : NancyModule
    {
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

        private dynamic Get_ExecuteTransaction(dynamic arg)
        {
            return View[new TransactionViewModel()];
        }

        private dynamic Post_ExecuteTransaction(dynamic arg)
        {
            _session.Advanced.UseOptimisticConcurrency = true;

            Guid uniqueId = (Context.CurrentUser as User).UniqueId;
            var viewModel = this.Bind<TransactionViewModel>();

            User user = _session.Query<User>().Where(u => u.UniqueId == uniqueId).FirstOrDefault();
            User receiverUser = _session.Query<User>().Where(u => u.Email == viewModel.Email).FirstOrDefault();

            viewModel.AmountInvalid = viewModel.Amount <= 0;
            viewModel.NotEnoughMoney = viewModel.Amount > user.Amount;
            viewModel.EmailHasError = receiverUser == null;

            if (viewModel.AmountInvalid || viewModel.NotEnoughMoney || viewModel.EmailHasError)
            {
                return View[viewModel];
            }

            receiverUser.Amount += viewModel.Amount;
            user.Amount -= viewModel.Amount;

            var transaction = new Transaction(user, receiverUser, viewModel.Amount);
            _session.Store(transaction);

            try
            {
                _session.SaveChanges();
            }
            catch (ConcurrencyException ex)
            {
                viewModel.TransactionFailed = true;
                return View[viewModel];
            }

            return Response.AsRedirect("~/user");
        }

        private dynamic Post_ChangePassword(dynamic arg)
        {
            Guid uniqueId = (Context.CurrentUser as User).UniqueId;
            var viewModel = this.Bind<ChangePasswordViewModel>();

            User user = _session.Query<User>().Where(u => u.UniqueId == uniqueId).FirstOrDefault();

            viewModel.CurrentPasswordHasError = user.Password != _userMapper.CreateHash(viewModel.CurrentPassword);
            viewModel.RepeatPasswordHasError = viewModel.NewPassword != viewModel.RepeatPassword;
            viewModel.NewPasswordHasError = viewModel.NewPassword == null;

            if (viewModel.CurrentPasswordHasError || viewModel.RepeatPasswordHasError || viewModel.NewPasswordHasError)
            {
                return View[viewModel];
            }

            user.Password = _userMapper.CreateHash(viewModel.NewPassword);
            _session.SaveChanges();
            return Response.AsRedirect("~/user");
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

            public bool CurrentPasswordHasError { get; set; }
            public bool RepeatPasswordHasError { get; set; }
            public bool NewPasswordHasError { get; set; }
        }

        public class TransactionViewModel
        {
            public string Email { get; set; }
            public float Amount { get; set; }

            public bool EmailHasError { get; set; }
            public bool NotEnoughMoney { get; set; }
            public bool AmountInvalid { get; set; }
            public bool TransactionFailed { get; set; }
        }

    }
}