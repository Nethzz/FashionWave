namespace RegClient.Models
{
    public class Registration_Modify
    {
        public Guid Id { get; set; }  // Shared Id for both tables

        // PersonalDetails properties
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public string ImagePath { get; set; }

        // UserCredentials properties
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; } = "User";  // Default UserType value
        public string Status { get; set; } = "Active";  // Default Status value
        public string Profile { get; set; }

        public Registration_Modify()
        {
            UserType = "User";  // Default value for UserType
            Status = "Active";  // Default value for Status
        }
    }
}
