using MetricsApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.ViewModels
{
    public class BestUserPartialModel
    {
        public UserWithCounter UserWithLargestIssuesClosed { get; set; }
    }
}