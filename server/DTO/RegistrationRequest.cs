using RegApi.Model;

namespace RegApi.DTO
{
    public class RegistrationRequest
    {
        public PersonalDetails PersonalDetails { get; set; }
        public UserCredentials UserCredentials { get; set; }
    }
}
