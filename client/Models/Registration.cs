﻿using System.ComponentModel.DataAnnotations;

namespace RegClient.Models
{
    public class Registration
    {
        [Key]
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public string ImagePath { get; set; }



    }
}
