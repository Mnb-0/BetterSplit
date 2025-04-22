using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BetterSplit.Models
{
    public class Group
    {
        public Group()
        {
            Members = new List<Member>();
            Expenses = new List<Expense>();
        }

        public string Name { get; set; }
        public List<Member> Members { get; set; }
        public List<Expense> Expenses { get; set; }

        public Group(string name)
        {
            Name = name;
            Members = new List<Member>();
            Expenses = new List<Expense>();
        }

        public void AddMember(Member member)
        {
            Members.Add(member);
        }

        public void AddExpense(Expense expense)
        {
            Expenses.Add(expense);
        }


    }
}