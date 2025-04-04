﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.Models
{
    public class Order
    {
        public int OrderID { get; set; }

        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public int? ShipperID { get; set; }
        public virtual Shipper? Shipper { get; set; }

        public int? EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }

        public decimal? TotalCharge { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();
    }
}
