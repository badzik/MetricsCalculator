using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.Models
{
    public class UserWithCounter
    {
        public User User { get; set; }
        public int Counter { get; set; }

        public static UserWithCounter GetUserWithLargestCounter(List<UserWithCounter> users)
        {
            if (users == null || users.Count < 1)
            {
                return new UserWithCounter()
                {
                    Counter = 0,
                    User = new User()
                };
            }
            UserWithCounter result = new UserWithCounter()
            {
                User = null,
                Counter = 0
            };
            foreach (UserWithCounter u in users)
            {
                if (result.Counter <= u.Counter)
                {
                    result = u;
                }
            }
            return result;
        }
    }
}