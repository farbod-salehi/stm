using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Application.DTOs.Auth
{
    public class RegisterUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

}
