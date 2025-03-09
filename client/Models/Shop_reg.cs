namespace RegClient.Models
{
    public class Shop_reg
    {
        public Guid Id { get; set; }
        public string ShopLicens { get; set; }
        public string ShopName { get; set; }
        public string ShopType { get; set; }
        public string ShopEmail { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; } = "ShpOwner";  // Default UserType value
        public string Status { get; set; } = "Active";
    }
}
