using STM.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace STM.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = default!;

        public string PasswordHash { get; set; } = default!;

        public UserStatus Status { get; set; } = UserStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? VerifiedAt { get; set; } = default;

        public string? Title { get; set; } = default;

        public DateTime? LastLoginAt { get; set; } = default;

        public void Activate()
        {
            if (Status != UserStatus.Pending)
                throw new InvalidOperationException("");

            Status = UserStatus.Active;
        }
    }
}
