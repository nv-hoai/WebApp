using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FastFood.MVC.Models
{
    public enum OrderStatus 
    { 
        Pending,    //Mới tạo đơn
        Processing, //Đang xử lý
        Delivering, //Đang giao
        Completed,  //Đã hoàn tất
        Cancelled   //Đã hủy
    }
    public enum ShippingMethod 
    {
        Express,    //Ưu tiên
        Fast,       //Nhanh
        Economy     //Tiết kiệm
    }
    public class Order
    {
        public int OrderID { get; set; }

        public int CustomerID { get; set; }
        public virtual Customer Customer { get; set; } = null!;

        public int? ShipperID { get; set; }
        public virtual Shipper? Shipper { get; set; }

        public int? EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }
        public string Address { get; set; }
		public ShippingMethod ShippingMethod { get; set; }
		public decimal ShippingFee { get; set; }

		[Column(TypeName = "decimal(18,2)")]
        public decimal? TotalCharge { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new HashSet<OrderDetail>();

        public decimal SubTotal { get; set; }
		public decimal GetShippingFee()
		{
			return ShippingMethod switch
			{
				ShippingMethod.Express => 50.00m,
				ShippingMethod.Fast => 40.00m, 
				ShippingMethod.Economy => 30.00m,
				_ => 40.00m   
			};
		}
		public void CalculateTotalCharge()
        {
			foreach (var detail in OrderDetails)
			{
				detail.CalculatePrices();
			}
			ShippingFee = GetShippingFee();
            SubTotal = OrderDetails.Sum(od => od.SubTotal);
            TotalCharge = SubTotal + ShippingFee;
        }
    }
}
