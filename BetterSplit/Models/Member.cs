using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterSplit.Models
{
    public class Member
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }


        public Member()
        {
            Balance = 0;
            Name = string.Empty;
        }

        public Member(string name)
        {
            Name = name;
            Balance = 0; // Default balance is 0
        }

        public void SettleBalance(decimal amount)
        {
            Balance -= amount;
        }

    }
}

