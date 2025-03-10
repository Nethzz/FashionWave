﻿namespace RegApi.DTO
{
    public class UserDetailsDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public string UserType { get; set; }
        public string Status { get; set; }
        public string Password { get; set; }
        public string Imagepath { get; set; }
    }
}
