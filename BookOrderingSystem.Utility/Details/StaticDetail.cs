using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookOrderingSystem.Utility.Details
{
    public class StaticDetail
    {
        public class Role
        {
            public const string IndividualUser = "Individual";
            public const string CompanyUser = "Company";
            public const string Admin = "Admin";
            public const string Employee = "Employee";
        }
        
        public class Order
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Processing = "Processing";
            public const string Shipped = "Shipped";
            public const string Cancelled = "Cancelled";
            public const string Refunded = "Refunded";
        }

        public class Payment
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Delayed = "Delayed";
            public const string Rejected = "Rejected";
        }
    }
}
