using System.ComponentModel.DataAnnotations;

namespace RegApi.Model
{
    public class PersonalDetails
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public string? Imagepath { get; set; }
    }
}
