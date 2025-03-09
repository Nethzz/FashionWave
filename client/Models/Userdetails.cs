namespace RegClient.Models
{
    public class Userdetails
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; } = "User";  // Default UserType value
        public string Status { get; set; } = "Active";  // Default Status value

    }
}
