using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BetterSplit.Models
{
    public class Expense
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public Member Payer { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        // Constructor to make it easier to create Expense objects
        public Expense(string description, decimal amount, Member payer)
        {
            Description = description;
            Amount = amount;
            Payer = payer;
        }



    }
}
