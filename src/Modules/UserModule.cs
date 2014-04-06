using Nancy;
using Nancy.Security;
using NancyBoilerplate.Web.Entities;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NancyBoilerplate.Web.Modules
{
    public class UserModule : NancyModule
    {
        private readonly IDocumentSession _session;

        public UserModule(IDocumentSession session) : base("/user")
        {
            _session = session;
            
            this.RequiresClaims(new[] { "user" });

            Get["/"] = Get_Transactions;


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

    }
}