using RegApi.Model;

namespace RegApi.DTO
{
    public class ShopRegistration
    {
        public ShopOwner ShopOwners { get; set; }
        public UserCredentials UserCredentials { get; set; }
    }
}
