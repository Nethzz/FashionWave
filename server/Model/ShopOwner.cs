using System.ComponentModel.DataAnnotations;

namespace RegApi.Model
{
    public class ShopOwner
    {
        [Key]
        public Guid Id { get; set; }
        public string ShopLicens { get; set; }
        public string ShopName { get; set; }
        public string ShopType { get; set; }
        public string ShopEmail { get; set; }
       
    }
}
