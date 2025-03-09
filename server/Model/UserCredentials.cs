using System.ComponentModel.DataAnnotations;

namespace RegApi.Model
{
    public class UserCredentials
    {
        [Key]
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
        public string Status { get; set; } // New Status property
    }
}
