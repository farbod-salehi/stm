using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Entities
{
    public class OtpCode
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Code { get; set; }
        public DateTime ExpiresAt { get; set; }

        public bool IsExpired(DateTime now) => now > ExpiresAt;
    }
}
