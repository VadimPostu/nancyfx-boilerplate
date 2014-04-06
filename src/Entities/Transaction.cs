using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NancyBoilerplate.Web.Entities
{
    public class Transaction
    {
        public int Id { get; set; }

        public TransactionParty Sender { get; set; }
        public TransactionParty Receiver { get; set; }

        public float Amount { get; set; }
        public DateTime Date { get; set; }

        public Transaction()
        {
        }

        public Transaction(User from, User to, float amount)
        {
            Sender = new TransactionParty(from);
            Receiver = new TransactionParty(to);
            Amount = amount;
            Date = DateTime.Now;
        }
    }

    public class TransactionParty
    {
        public Guid UniqueId { get; set; }
        public string Email { get; set; }

        public TransactionParty()
        {
        }

        public TransactionParty(User user)
        {
            UniqueId = user.UniqueId;
            Email = user.Email;
        }
    }
}