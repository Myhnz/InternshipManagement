using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InternshipManagement.Models
{
    public class DataTableParameters
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Search Search { get; set; }
        public List<Order> Order { get; set; } // Thêm thuộc tính cho thông tin sắp xếp

        // You may add other properties as needed
    }

    public class Search
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
        // You may add other properties as needed
    }

    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; }
        public string Name { get; set; } // New property for sorting column name
    }


}