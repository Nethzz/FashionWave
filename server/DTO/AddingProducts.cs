using RegApi.Model;

namespace RegApi.DTO
{
    public class AddingProducts
    {
        //Session id
        public Guid Id { get; set; }
        public Guid Pid { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public string ProductDescription { get; set; }

        public string? ProductImg { get; set; }
        public string? Payment { get; set; }

    }
}
