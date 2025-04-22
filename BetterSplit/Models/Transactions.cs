using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterSplit.Models
{
    public class Transaction
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public Member Payer { get; set; }
        public Member Payee { get; set; }
        public DateTime Date { get; set; }

        public Transaction(string description, decimal amount, Member payer, Member payee)
        {
            Description = description;
            Amount = amount;
            Payer = payer;
            Payee = payee;
            Date = DateTime.Now;
        }
    }
}
