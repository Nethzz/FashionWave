namespace RegApi.DTO
{
    public class ShopDetailsDto
    {
        public Guid Id { get; set; }
        public string ShopLicens { get; set; }
        public string ShopName { get; set; }
        public string ShopType { get; set; }
        public string ShopEmail { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
        public string Status { get; set; } // New Status property

        public List<AddingProducts> Products { get; set; }
    }
}

