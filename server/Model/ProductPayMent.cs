namespace RegApi.Model
{
    public class ProductPayMent
    {
        public Guid Id { get; set; }    
        public Guid Pid { get; set; }
        public Guid Uid { get; set; }
        public Guid Cid { get; set; }
        public string Payment { get; set; }
        public string Address { get; set; }
        public string Amount { get; set; }

    }
}
