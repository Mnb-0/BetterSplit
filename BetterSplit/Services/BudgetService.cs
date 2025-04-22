using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterSplit.Models;

namespace BetterSplit.Services
{
    public class BudgetService
    {
        private Dictionary<string, string> users = new Dictionary<string, string>();
        private List<Group> groups = new List<Group>();
        private string loggedInUser = null;

        public bool Login(string username, string password)
        {
            if (users.ContainsKey(username) && users[username] == password)
            {
                loggedInUser = username;
                return true;
            }
            return false;
        }


        public bool SignUp(string username, string password)
        {
            if (users.ContainsKey(username))
            {
                return false;
            }
            users[username] = password;
            return true;
        }

        public void CreateGroup(string groupName)
        {
            Group group = new Group(groupName);
            if (loggedInUser != null)
            {
                group.Members.Add(new Member(loggedInUser));
            }
            groups.Add(group);
        }



        public List<Group> GetGroups()
        {
            return groups;
        }
    }
}