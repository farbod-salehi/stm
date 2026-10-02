using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Application.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

}
