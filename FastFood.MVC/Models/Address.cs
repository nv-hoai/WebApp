namespace FastFood.MVC.Models
{
    public class Address
    {
        public int AddressID { get; set; }

        public string HouseNumber { get; set; } = null!;
        public string StreetName { get; set; } = null!;
        public string District { get; set; } = null!;
        public string City { get; set; } = null!;
    }
}
